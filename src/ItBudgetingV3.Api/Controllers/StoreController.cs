using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/store")]
public sealed class StoreController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpGet("cases/{customerReference}")]
    public async Task<ActionResult<CaseSummaryResponse>> ResumeByCustomerReference(string customerReference, CancellationToken cancellationToken)
        => await onboardingService.GetCaseByCustomerReferenceAsync(customerReference, cancellationToken) is { } response ? Ok(response) : NotFound();

    [HttpPost("cases/{caseId:guid}/assisted-transition")]
    public async Task<ActionResult<CaseSummaryResponse>> ContinueAssisted(Guid caseId, ChannelTransitionRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.TransitionChannelAsync(caseId, request, cancellationToken));
}
