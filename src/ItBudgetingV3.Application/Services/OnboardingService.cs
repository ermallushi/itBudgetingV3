using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Providers;
using ItBudgetingV3.Domain.Entities;
using ItBudgetingV3.Domain.Enums;
using ItBudgetingV3.Domain.Interfaces;

namespace ItBudgetingV3.Application.Services;

public sealed class OnboardingService(
    IOnboardingCaseRepository repository,
    IWebhookEventStore webhookEventStore,
    IIdentityVerificationProvider identityVerificationProvider,
    IBiometricVerificationProvider biometricVerificationProvider,
    IRiskScreeningProvider riskScreeningProvider,
    IDigitalSignatureProvider digitalSignatureProvider) : IOnboardingService
{
    private const decimal ReviewThreshold = 50m;
    private const decimal RejectThreshold = 80m;

    public async Task<CaseSummaryResponse> CreateCaseAsync(CreateCaseRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = new OnboardingCase(request.ExternalReference, request.JourneyType, request.CurrentChannel, request.CustomerReference);
        await repository.AddAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse?> GetCaseAsync(Guid caseId, CancellationToken cancellationToken = default)
        => (await repository.GetAsync(caseId, cancellationToken)) is { } onboardingCase ? Map(onboardingCase) : null;

    public async Task<CaseSummaryResponse?> GetCaseByCustomerReferenceAsync(string customerReference, CancellationToken cancellationToken = default)
        => (await repository.GetByCustomerReferenceAsync(customerReference, cancellationToken)) is { } onboardingCase ? Map(onboardingCase) : null;

    public async Task<CaseSummaryResponse> SubmitPersonProfileAsync(Guid caseId, PersonProfileRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.SetPersonProfile(new PersonProfile
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Nationality = request.Nationality,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            ConsentFlags = request.ConsentFlags ?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        }, NewCorrelationId());

        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> SubmitIdentityDocumentAsync(Guid caseId, IdentityDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        var providerResponse = await identityVerificationProvider.SubmitAsync(caseId, cancellationToken);
        var document = new IdentityDocument
        {
            CaseId = onboardingCase.CaseId,
            DocumentType = request.DocumentType,
            IssuingCountry = request.IssuingCountry,
            DocumentNumber = request.DocumentNumber,
            ExpiryDate = request.ExpiryDate,
            FrontFileReference = request.FrontFileReference,
            BackFileReference = request.BackFileReference,
            NfcReference = request.NfcReference,
            ProviderReference = providerResponse.ProviderReference
        };

        onboardingCase.AddIdentityDocument(document, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> HandleDocumentVerificationCallbackAsync(Guid caseId, DocumentVerificationCallbackRequest request, CancellationToken cancellationToken = default)
    {
        if (!await webhookEventStore.TryRegisterAsync(request.ProviderEventId, cancellationToken))
        {
            return await GetExistingCaseAsync(caseId, cancellationToken);
        }

        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.CompleteDocumentVerification(request.DocumentId, request.IsSuccessful, request.AuthenticityResult, request.TamperResult, request.ExtractionPayload, request.CorrelationId);
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> SubmitBiometricAsync(Guid caseId, BiometricSessionRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        var providerResponse = await biometricVerificationProvider.SubmitAsync(caseId, cancellationToken);
        var biometricSession = new BiometricSession
        {
            CaseId = onboardingCase.CaseId,
            SelfieFileReference = request.SelfieFileReference,
            ProviderReference = providerResponse.ProviderReference
        };

        onboardingCase.AddBiometricSession(biometricSession, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> HandleBiometricCallbackAsync(Guid caseId, BiometricCallbackRequest request, CancellationToken cancellationToken = default)
    {
        if (!await webhookEventStore.TryRegisterAsync(request.ProviderEventId, cancellationToken))
        {
            return await GetExistingCaseAsync(caseId, cancellationToken);
        }

        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.CompleteBiometric(request.BiometricId, request.IsSuccessful, request.LivenessResult, request.FaceMatchScore, request.DecisionReason, request.CorrelationId);
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> RunRiskScreeningAsync(Guid caseId, RiskScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        var providerResponse = await riskScreeningProvider.SubmitAsync(caseId, cancellationToken);
        var riskResult = new RiskResult
        {
            CaseId = onboardingCase.CaseId,
            FraudScore = request.FraudScore,
            SanctionsResult = request.SanctionsResult,
            PepResult = request.PepResult,
            AdverseMediaResult = request.AdverseMediaResult,
            FinalRiskScore = request.FraudScore,
            ScreeningReference = providerResponse.ProviderReference,
            RecommendedAction = GetRecommendedAction(request.FraudScore)
        };

        onboardingCase.AddRiskResult(riskResult, NewCorrelationId());
        onboardingCase.ApplyRiskDecision(
            riskResult.RecommendedAction,
            GetAssignedQueue(riskResult.RecommendedAction),
            GetRiskLevel(riskResult.FraudScore),
            NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> CreateReviewCaseAsync(Guid caseId, ReviewRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.AddReviewCase(new ReviewCase
        {
            CaseId = onboardingCase.CaseId,
            QueueName = request.QueueName,
            Reason = request.Reason,
            AssignedUser = request.AssignedUser
        }, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> ResolveReviewCaseAsync(Guid caseId, Guid reviewId, ReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.ResolveReview(reviewId, request.Decision, request.Note, request.Reviewer, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> CreateSignaturePackageAsync(Guid caseId, SignaturePackageRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        var providerResponse = await digitalSignatureProvider.SubmitAsync(caseId, cancellationToken);
        onboardingCase.AddSignaturePackage(new SignaturePackage
        {
            CaseId = onboardingCase.CaseId,
            DocumentReference = request.DocumentReference,
            SignatureLevel = request.SignatureLevel,
            SignerAuthMethod = request.SignerAuthMethod,
            ProviderReference = providerResponse.ProviderReference,
            Status = providerResponse.Status
        }, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> HandleSignatureCallbackAsync(Guid caseId, SignatureCallbackRequest request, CancellationToken cancellationToken = default)
    {
        if (!await webhookEventStore.TryRegisterAsync(request.ProviderEventId, cancellationToken))
        {
            return await GetExistingCaseAsync(caseId, cancellationToken);
        }

        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.CompleteSignature(request.SignatureId, request.IsSigned, request.EvidenceReference, request.CorrelationId);
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> TransitionChannelAsync(Guid caseId, ChannelTransitionRequest request, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.TransitionChannel(request.ToChannel, request.Reason, request.PerformedBy, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<CaseSummaryResponse> CancelCaseAsync(Guid caseId, string actorId, CancellationToken cancellationToken = default)
    {
        var onboardingCase = await RequireCaseAsync(caseId, cancellationToken);
        onboardingCase.Cancel(actorId, NewCorrelationId());
        await repository.UpdateAsync(onboardingCase, cancellationToken);
        return Map(onboardingCase);
    }

    public async Task<OperationalMetricsResponse> GetOperationalMetricsAsync(CancellationToken cancellationToken = default)
    {
        var cases = await repository.ListAsync(cancellationToken);
        var grouped = cases.GroupBy(c => c.Status).ToDictionary(g => g.Key, g => g.Count());

        return new OperationalMetricsResponse(
            cases.Count,
            cases.Count(c => c.Status == CaseStatus.Completed),
            cases.Count(c => c.Status == CaseStatus.Rejected),
            cases.Count(c => c.Status == CaseStatus.RiskReview),
            grouped);
    }

    private async Task<OnboardingCase> RequireCaseAsync(Guid caseId, CancellationToken cancellationToken)
        => await repository.GetAsync(caseId, cancellationToken) ?? throw new KeyNotFoundException($"Case {caseId} was not found.");

    private async Task<CaseSummaryResponse> GetExistingCaseAsync(Guid caseId, CancellationToken cancellationToken)
        => Map(await RequireCaseAsync(caseId, cancellationToken));

    private static string NewCorrelationId() => Guid.NewGuid().ToString("N");

    private static string GetAssignedQueue(string recommendedAction)
        => string.Equals(recommendedAction, "review", StringComparison.OrdinalIgnoreCase)
            ? "ComplianceReview"
            : string.Empty;

    private static string GetRecommendedAction(decimal fraudScore)
        => fraudScore >= RejectThreshold
            ? "reject"
            : fraudScore >= ReviewThreshold
                ? "review"
                : "approve";

    private static string GetRiskLevel(decimal fraudScore)
        => fraudScore >= RejectThreshold
            ? "High"
            : fraudScore >= ReviewThreshold
                ? "Medium"
                : "Low";

    private static CaseSummaryResponse Map(OnboardingCase onboardingCase)
        => new(
            onboardingCase.CaseId,
            onboardingCase.ExternalReference,
            onboardingCase.JourneyType,
            onboardingCase.CurrentChannel,
            onboardingCase.PreviousChannel,
            onboardingCase.Status,
            onboardingCase.SubStatus,
            onboardingCase.CustomerReference,
            onboardingCase.AssignedQueue,
            onboardingCase.RiskLevel,
            onboardingCase.FinalDecision,
            onboardingCase.CreatedAt,
            onboardingCase.UpdatedAt,
            onboardingCase.PersonProfile,
            onboardingCase.IdentityDocuments.AsReadOnly(),
            onboardingCase.BiometricSessions.AsReadOnly(),
            onboardingCase.RiskResults.AsReadOnly(),
            onboardingCase.ReviewCases.AsReadOnly(),
            onboardingCase.SignaturePackages.AsReadOnly(),
            onboardingCase.AuditEvents.AsReadOnly(),
            onboardingCase.ChannelTransitions.AsReadOnly());
}
