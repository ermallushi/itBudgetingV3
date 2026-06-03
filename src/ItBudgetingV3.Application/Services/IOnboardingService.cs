using ItBudgetingV3.Application.Contracts;

namespace ItBudgetingV3.Application.Services;

public interface IOnboardingService
{
    Task<CaseSummaryResponse> CreateCaseAsync(CreateCaseRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse?> GetCaseAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse?> GetCaseByCustomerReferenceAsync(string customerReference, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> SubmitPersonProfileAsync(Guid caseId, PersonProfileRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> SubmitIdentityDocumentAsync(Guid caseId, IdentityDocumentRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> HandleDocumentVerificationCallbackAsync(Guid caseId, DocumentVerificationCallbackRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> SubmitBiometricAsync(Guid caseId, BiometricSessionRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> HandleBiometricCallbackAsync(Guid caseId, BiometricCallbackRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> RunRiskScreeningAsync(Guid caseId, RiskScreeningRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> CreateReviewCaseAsync(Guid caseId, ReviewRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> ResolveReviewCaseAsync(Guid caseId, Guid reviewId, ReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> CreateSignaturePackageAsync(Guid caseId, SignaturePackageRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> HandleSignatureCallbackAsync(Guid caseId, SignatureCallbackRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> TransitionChannelAsync(Guid caseId, ChannelTransitionRequest request, CancellationToken cancellationToken = default);
    Task<CaseSummaryResponse> CancelCaseAsync(Guid caseId, string actorId, CancellationToken cancellationToken = default);
    Task<OperationalMetricsResponse> GetOperationalMetricsAsync(CancellationToken cancellationToken = default);
}
