using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost("cases/{caseId:guid}/document-verifications")]
    public async Task<ActionResult<CaseSummaryResponse>> DocumentVerification(Guid caseId, DocumentVerificationCallbackRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.HandleDocumentVerificationCallbackAsync(caseId, request, cancellationToken));

    [HttpPost("cases/{caseId:guid}/biometrics")]
    public async Task<ActionResult<CaseSummaryResponse>> Biometric(Guid caseId, BiometricCallbackRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.HandleBiometricCallbackAsync(caseId, request, cancellationToken));

    [HttpPost("cases/{caseId:guid}/signatures")]
    public async Task<ActionResult<CaseSummaryResponse>> Signature(Guid caseId, SignatureCallbackRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.HandleSignatureCallbackAsync(caseId, request, cancellationToken));
}
