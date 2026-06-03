using ItBudgetingV3.Domain.Enums;
using ItBudgetingV3.Domain.Exceptions;

namespace ItBudgetingV3.Domain.Entities;

public sealed class OnboardingCase
{
    private static readonly IReadOnlyDictionary<CaseStatus, CaseStatus[]> AllowedTransitions = new Dictionary<CaseStatus, CaseStatus[]>
    {
        [CaseStatus.Started] = [CaseStatus.IdentityCaptured, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.IdentityCaptured] = [CaseStatus.DocumentVerificationPending, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.DocumentVerificationPending] = [CaseStatus.DocumentVerified, CaseStatus.DocumentRejected, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.DocumentVerified] = [CaseStatus.BiometricPending, CaseStatus.RiskPending, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.DocumentRejected] = [CaseStatus.Rejected, CaseStatus.Cancelled],
        [CaseStatus.BiometricPending] = [CaseStatus.BiometricVerified, CaseStatus.BiometricFailed, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.BiometricVerified] = [CaseStatus.RiskPending, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.BiometricFailed] = [CaseStatus.Rejected, CaseStatus.Cancelled],
        [CaseStatus.RiskPending] = [CaseStatus.RiskReview, CaseStatus.RiskRejected, CaseStatus.ApprovedForSigning, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.RiskReview] = [CaseStatus.ApprovedForSigning, CaseStatus.RiskRejected, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.RiskRejected] = [CaseStatus.Rejected, CaseStatus.Cancelled],
        [CaseStatus.ApprovedForSigning] = [CaseStatus.SignaturePending, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.SignaturePending] = [CaseStatus.Signed, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.Signed] = [CaseStatus.Completed],
        [CaseStatus.Completed] = [],
        [CaseStatus.Rejected] = [],
        [CaseStatus.Expired] = [],
        [CaseStatus.Cancelled] = []
    };

    public Guid CaseId { get; init; } = Guid.NewGuid();
    public string ExternalReference { get; set; } = string.Empty;
    public JourneyType JourneyType { get; set; }
    public ChannelType CurrentChannel { get; private set; }
    public ChannelType? PreviousChannel { get; private set; }
    public CaseStatus Status { get; private set; } = CaseStatus.Started;
    public string SubStatus { get; private set; } = string.Empty;
    public string CustomerReference { get; set; } = string.Empty;
    public string AssignedQueue { get; private set; } = string.Empty;
    public string RiskLevel { get; private set; } = string.Empty;
    public FinalDecision FinalDecision { get; private set; } = FinalDecision.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public PersonProfile? PersonProfile { get; private set; }
    public List<IdentityDocument> IdentityDocuments { get; } = [];
    public List<BiometricSession> BiometricSessions { get; } = [];
    public List<RiskResult> RiskResults { get; } = [];
    public List<ReviewCase> ReviewCases { get; } = [];
    public List<SignaturePackage> SignaturePackages { get; } = [];
    public List<AuditEvent> AuditEvents { get; } = [];
    public List<ChannelTransition> ChannelTransitions { get; } = [];

    public OnboardingCase(string externalReference, JourneyType journeyType, ChannelType currentChannel, string customerReference)
    {
        ExternalReference = externalReference;
        JourneyType = journeyType;
        CurrentChannel = currentChannel;
        CustomerReference = customerReference;
    }

    public void SetPersonProfile(PersonProfile profile, string correlationId)
    {
        PersonProfile = profile;
        TransitionTo(CaseStatus.IdentityCaptured, ActorType.Customer, profile.PersonId.ToString(), "PersonProfileCaptured", correlationId);
    }

    public void AddIdentityDocument(IdentityDocument document, string correlationId)
    {
        IdentityDocuments.Add(document);
        TransitionTo(CaseStatus.DocumentVerificationPending, ActorType.Customer, document.DocumentId.ToString(), "DocumentVerificationRequested", correlationId);
    }

    public void CompleteDocumentVerification(Guid documentId, bool success, string authenticityResult, string tamperResult, string extractionPayload, string correlationId)
    {
        var document = IdentityDocuments.First(d => d.DocumentId == documentId);
        document.AuthenticityResult = authenticityResult;
        document.TamperResult = tamperResult;
        document.ExtractionPayload = extractionPayload;
        TransitionTo(success ? CaseStatus.DocumentVerified : CaseStatus.DocumentRejected, ActorType.Provider, document.ProviderReference, "DocumentVerificationCompleted", correlationId);
    }

    public void AddBiometricSession(BiometricSession biometricSession, string correlationId)
    {
        BiometricSessions.Add(biometricSession);
        TransitionTo(CaseStatus.BiometricPending, ActorType.Customer, biometricSession.BiometricId.ToString(), "BiometricVerificationRequested", correlationId);
    }

    public void CompleteBiometric(Guid biometricId, bool success, string livenessResult, decimal faceMatchScore, string decisionReason, string correlationId)
    {
        var session = BiometricSessions.First(b => b.BiometricId == biometricId);
        session.LivenessResult = livenessResult;
        session.FaceMatchScore = faceMatchScore;
        session.DecisionReason = decisionReason;
        TransitionTo(success ? CaseStatus.BiometricVerified : CaseStatus.BiometricFailed, ActorType.Provider, session.ProviderReference, "BiometricVerificationCompleted", correlationId);
    }

    public void AddRiskResult(RiskResult riskResult, string correlationId)
    {
        RiskResults.Add(riskResult);
        TransitionTo(CaseStatus.RiskPending, ActorType.System, riskResult.RiskId.ToString(), "RiskScreeningRequested", correlationId);
    }

    public void ApplyRiskDecision(string recommendedAction, string queueName, string riskLevel, string correlationId)
    {
        RiskLevel = riskLevel;
        AssignedQueue = queueName;

        var nextStatus = recommendedAction.ToLowerInvariant() switch
        {
            "approve" => CaseStatus.ApprovedForSigning,
            "review" => CaseStatus.RiskReview,
            _ => CaseStatus.RiskRejected
        };

        FinalDecision = nextStatus switch
        {
            CaseStatus.ApprovedForSigning => FinalDecision.Approved,
            CaseStatus.RiskReview => FinalDecision.ManualReview,
            _ => FinalDecision.Rejected
        };

        TransitionTo(nextStatus, ActorType.System, "risk-engine", "RiskDecisionApplied", correlationId);
    }

    public ReviewCase AddReviewCase(ReviewCase reviewCase, string correlationId)
    {
        ReviewCases.Add(reviewCase);
        AssignedQueue = reviewCase.QueueName;
        AppendAuditEvent(ActorType.Reviewer, reviewCase.AssignedUser, "ReviewCaseCreated", correlationId, reviewCase.Reason);
        return reviewCase;
    }

    public void ResolveReview(Guid reviewId, string decision, string note, string reviewer, string correlationId)
    {
        var reviewCase = ReviewCases.First(r => r.ReviewId == reviewId);
        reviewCase.OverrideDecision = decision;
        reviewCase.Notes.Add(note);
        reviewCase.ResolvedAt = DateTimeOffset.UtcNow;

        var nextStatus = decision.ToLowerInvariant() == "approve"
            ? CaseStatus.ApprovedForSigning
            : CaseStatus.Rejected;

        FinalDecision = nextStatus == CaseStatus.ApprovedForSigning ? FinalDecision.Approved : FinalDecision.Rejected;
        TransitionTo(nextStatus, ActorType.Reviewer, reviewer, "ReviewDecisionApplied", correlationId);
    }

    public void AddSignaturePackage(SignaturePackage package, string correlationId)
    {
        SignaturePackages.Add(package);
        TransitionTo(CaseStatus.SignaturePending, ActorType.System, package.SignatureId.ToString(), "SignaturePackageCreated", correlationId);
    }

    public void CompleteSignature(Guid signatureId, bool signed, string evidenceReference, string correlationId)
    {
        var package = SignaturePackages.First(s => s.SignatureId == signatureId);
        package.EvidenceReference = evidenceReference;
        package.Status = signed ? "SIGNED" : "REJECTED";
        package.SignedAt = signed ? DateTimeOffset.UtcNow : null;

        if (!signed)
        {
            TransitionTo(CaseStatus.Rejected, ActorType.Provider, package.ProviderReference, "SignatureRejected", correlationId);
            FinalDecision = FinalDecision.Rejected;
            return;
        }

        TransitionTo(CaseStatus.Signed, ActorType.Provider, package.ProviderReference, "SignatureCompleted", correlationId);
        TransitionTo(CaseStatus.Completed, ActorType.System, "orchestrator", "CaseCompleted", correlationId);
    }

    public void TransitionChannel(ChannelType toChannel, string reason, string performedBy, string correlationId)
    {
        var transition = new ChannelTransition
        {
            CaseId = CaseId,
            FromChannel = CurrentChannel,
            ToChannel = toChannel,
            TransitionReason = reason,
            PerformedBy = performedBy
        };

        PreviousChannel = CurrentChannel;
        CurrentChannel = toChannel;
        ChannelTransitions.Add(transition);
        UpdatedAt = DateTimeOffset.UtcNow;
        AppendAuditEvent(ActorType.Staff, performedBy, "ChannelTransitioned", correlationId, reason);
    }

    public void Cancel(string actorId, string correlationId)
        => TransitionTo(CaseStatus.Cancelled, ActorType.Staff, actorId, "CaseCancelled", correlationId);

    private void TransitionTo(CaseStatus newStatus, ActorType actorType, string actorId, string eventType, string correlationId)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var validStatuses) || !validStatuses.Contains(newStatus))
        {
            throw new InvalidCaseTransitionException($"Invalid transition from {Status} to {newStatus}.");
        }

        Status = newStatus;
        SubStatus = eventType;
        UpdatedAt = DateTimeOffset.UtcNow;
        AppendAuditEvent(actorType, actorId, eventType, correlationId, newStatus.ToString());
    }

    private void AppendAuditEvent(ActorType actorType, string actorId, string eventType, string correlationId, string payloadReference)
    {
        AuditEvents.Add(new AuditEvent
        {
            CaseId = CaseId,
            ActorType = actorType,
            ActorId = actorId,
            EventType = eventType,
            CorrelationId = correlationId,
            PayloadReference = payloadReference
        });
    }
}
