using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases/{caseId:guid}/risk")]
public sealed class RiskController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost("screen")]
    public async Task<ActionResult<CaseSummaryResponse>> RunScreening(Guid caseId, RiskScreeningRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.RunRiskScreeningAsync(caseId, request, cancellationToken));
}
