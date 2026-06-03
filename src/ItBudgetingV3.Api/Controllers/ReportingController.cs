using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/reporting")]
public sealed class ReportingController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<ActionResult<OperationalMetricsResponse>> GetMetrics(CancellationToken cancellationToken)
        => Ok(await onboardingService.GetOperationalMetricsAsync(cancellationToken));
}
