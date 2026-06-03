using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases/{caseId:guid}/signatures")]
public sealed class SignatureController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CaseSummaryResponse>> CreateSignaturePackage(Guid caseId, SignaturePackageRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.CreateSignaturePackageAsync(caseId, request, cancellationToken));
}
