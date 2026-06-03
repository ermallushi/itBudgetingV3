namespace ItBudgetingV3.Application.Providers;

public sealed record ProviderSubmissionResult(string ProviderReference, string Status);

public interface IIdentityVerificationProvider
{
    Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public interface IBiometricVerificationProvider
{
    Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public interface IRiskScreeningProvider
{
    Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public interface IDigitalSignatureProvider
{
    Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default);
}
