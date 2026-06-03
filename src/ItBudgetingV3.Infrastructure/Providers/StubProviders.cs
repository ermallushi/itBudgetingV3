using ItBudgetingV3.Application.Providers;

namespace ItBudgetingV3.Infrastructure.Providers;

public sealed class StubIdentityVerificationProvider : IIdentityVerificationProvider
{
    public Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ProviderSubmissionResult($"doc-{caseId:N}", "PENDING"));
}

public sealed class StubBiometricVerificationProvider : IBiometricVerificationProvider
{
    public Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ProviderSubmissionResult($"bio-{caseId:N}", "PENDING"));
}

public sealed class StubRiskScreeningProvider : IRiskScreeningProvider
{
    public Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ProviderSubmissionResult($"risk-{caseId:N}", "PENDING"));
}

public sealed class StubDigitalSignatureProvider : IDigitalSignatureProvider
{
    public Task<ProviderSubmissionResult> SubmitAsync(Guid caseId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ProviderSubmissionResult($"sig-{caseId:N}", "PENDING"));
}
