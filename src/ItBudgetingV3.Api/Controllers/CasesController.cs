using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CasesController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CaseSummaryResponse>> CreateCase(CreateCaseRequest request, CancellationToken cancellationToken)
        => CreatedAtAction(nameof(GetCase), new { caseId = (await onboardingService.CreateCaseAsync(request, cancellationToken)).CaseId }, await onboardingService.CreateCaseAsync(request, cancellationToken));

    [HttpGet("{caseId:guid}")]
    public async Task<ActionResult<CaseSummaryResponse>> GetCase(Guid caseId, CancellationToken cancellationToken)
        => await onboardingService.GetCaseAsync(caseId, cancellationToken) is { } response ? Ok(response) : NotFound();

    [HttpGet("by-customer/{customerReference}")]
    public async Task<ActionResult<CaseSummaryResponse>> GetCaseByCustomerReference(string customerReference, CancellationToken cancellationToken)
        => await onboardingService.GetCaseByCustomerReferenceAsync(customerReference, cancellationToken) is { } response ? Ok(response) : NotFound();

    [HttpPost("{caseId:guid}/cancel")]
    public async Task<ActionResult<CaseSummaryResponse>> Cancel(Guid caseId, [FromQuery] string actorId, CancellationToken cancellationToken)
        => Ok(await onboardingService.CancelCaseAsync(caseId, actorId, cancellationToken));

    [HttpPost("{caseId:guid}/transitions")]
    public async Task<ActionResult<CaseSummaryResponse>> TransitionChannel(Guid caseId, ChannelTransitionRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.TransitionChannelAsync(caseId, request, cancellationToken));
}
