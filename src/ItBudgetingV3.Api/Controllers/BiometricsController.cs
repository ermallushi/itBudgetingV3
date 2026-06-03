using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases/{caseId:guid}/biometrics")]
public sealed class BiometricsController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CaseSummaryResponse>> SubmitBiometric(Guid caseId, BiometricSessionRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.SubmitBiometricAsync(caseId, request, cancellationToken));
}
