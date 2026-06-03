using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using ItBudgetingV3.Domain.Enums;
using ItBudgetingV3.Domain.Exceptions;
using ItBudgetingV3.Domain.Interfaces;
using ItBudgetingV3.Infrastructure.Persistence;
using ItBudgetingV3.Infrastructure.Providers;

namespace ItBudgetingV3.Tests;

public sealed class OnboardingServiceTests
{
    private readonly IOnboardingService _service;

    public OnboardingServiceTests()
    {
        IOnboardingCaseRepository repository = new InMemoryOnboardingCaseRepository();
        IWebhookEventStore webhookEventStore = new InMemoryWebhookEventStore();
        _service = new OnboardingService(
            repository,
            webhookEventStore,
            new StubIdentityVerificationProvider(),
            new StubBiometricVerificationProvider(),
            new StubRiskScreeningProvider(),
            new StubDigitalSignatureProvider());
    }

    [Fact]
    public async Task FullDigitalJourney_CompletesCaseAndWritesAuditTrail()
    {
        var created = await _service.CreateCaseAsync(new CreateCaseRequest("EXT-1", JourneyType.Digital, ChannelType.CustomerWeb, "CUST-1"));
        await _service.SubmitPersonProfileAsync(created.CaseId, new PersonProfileRequest("Jane", "Doe", new DateOnly(1990, 1, 1), "AL", "jane@example.com", "+355123", "Tirana", new Dictionary<string, bool> { ["terms"] = true }));
        var withDocument = await _service.SubmitIdentityDocumentAsync(created.CaseId, new IdentityDocumentRequest("Passport", "AL", "A12345", new DateOnly(2030, 1, 1), "front-ref", "back-ref", "nfc-ref"));
        var documentId = withDocument.IdentityDocuments.Single().DocumentId;
        await _service.HandleDocumentVerificationCallbackAsync(created.CaseId, new DocumentVerificationCallbackRequest(documentId, true, "PASS", "CLEAR", "{}", "doc-event-1", "corr-doc"));
        var withBiometric = await _service.SubmitBiometricAsync(created.CaseId, new BiometricSessionRequest("selfie-ref"));
        var biometricId = withBiometric.BiometricSessions.Single().BiometricId;
        await _service.HandleBiometricCallbackAsync(created.CaseId, new BiometricCallbackRequest(biometricId, true, "PASS", 99m, "Matched", "bio-event-1", "corr-bio"));
        await _service.RunRiskScreeningAsync(created.CaseId, new RiskScreeningRequest(10m, "CLEAR", "CLEAR", "CLEAR"));
        var withSignature = await _service.CreateSignaturePackageAsync(created.CaseId, new SignaturePackageRequest("doc-package-ref", "advanced", "otp"));
        var signatureId = withSignature.SignaturePackages.Single().SignatureId;
        var completed = await _service.HandleSignatureCallbackAsync(created.CaseId, new SignatureCallbackRequest(signatureId, true, "evidence-ref", "sig-event-1", "corr-sig"));

        Assert.Equal(CaseStatus.Completed, completed.Status);
        Assert.Equal(FinalDecision.Approved, completed.FinalDecision);
        Assert.Contains(completed.AuditEvents, e => e.EventType == "CaseCompleted");
    }

    [Fact]
    public async Task DuplicateWebhook_IsIgnoredThroughIdempotencyStore()
    {
        var created = await _service.CreateCaseAsync(new CreateCaseRequest("EXT-2", JourneyType.Digital, ChannelType.CustomerWeb, "CUST-2"));
        await _service.SubmitPersonProfileAsync(created.CaseId, new PersonProfileRequest("John", "Smith", new DateOnly(1992, 2, 2), "AL", "john@example.com", "+355456", "Durres", null));
        var withDocument = await _service.SubmitIdentityDocumentAsync(created.CaseId, new IdentityDocumentRequest("ID", "AL", "B12345", new DateOnly(2031, 1, 1), "front", "back", "nfc"));
        var documentId = withDocument.IdentityDocuments.Single().DocumentId;

        var first = await _service.HandleDocumentVerificationCallbackAsync(created.CaseId, new DocumentVerificationCallbackRequest(documentId, true, "PASS", "CLEAR", "{}", "dup-doc-event", "corr-1"));
        var second = await _service.HandleDocumentVerificationCallbackAsync(created.CaseId, new DocumentVerificationCallbackRequest(documentId, false, "FAIL", "FAIL", "{}", "dup-doc-event", "corr-2"));

        Assert.Equal(CaseStatus.DocumentVerified, first.Status);
        Assert.Equal(CaseStatus.DocumentVerified, second.Status);
        Assert.Equal("PASS", second.IdentityDocuments.Single().AuthenticityResult);
    }

    [Fact]
    public void InvalidTransition_ThrowsDomainException()
    {
        var onboardingCase = new ItBudgetingV3.Domain.Entities.OnboardingCase("EXT-3", JourneyType.Digital, ChannelType.CustomerWeb, "CUST-3");

        var exception = Assert.Throws<InvalidCaseTransitionException>(() => onboardingCase.AddBiometricSession(new ItBudgetingV3.Domain.Entities.BiometricSession
        {
            CaseId = onboardingCase.CaseId,
            SelfieFileReference = "selfie",
            ProviderReference = "bio-ref"
        }, "corr"));

        Assert.Contains("Invalid transition", exception.Message);
    }

    [Fact]
    public async Task ChannelTransition_PreservesCrossChannelTraceability()
    {
        var created = await _service.CreateCaseAsync(new CreateCaseRequest("EXT-4", JourneyType.CrossChannel, ChannelType.CustomerMobile, "CUST-4"));
        var transitioned = await _service.TransitionChannelAsync(created.CaseId, new ChannelTransitionRequest(ChannelType.StorePortal, "Store continuation", "staff-1"));

        Assert.Equal(ChannelType.StorePortal, transitioned.CurrentChannel);
        Assert.Equal(ChannelType.CustomerMobile, transitioned.PreviousChannel);
        Assert.Single(transitioned.ChannelTransitions);
        Assert.Contains(transitioned.AuditEvents, e => e.EventType == "ChannelTransitioned");
    }
}
