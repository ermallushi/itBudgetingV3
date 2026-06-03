using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases/{caseId:guid}/reviews")]
public sealed class ReviewController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CaseSummaryResponse>> CreateReview(Guid caseId, ReviewRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.CreateReviewCaseAsync(caseId, request, cancellationToken));

    [HttpPost("{reviewId:guid}/decision")]
    public async Task<ActionResult<CaseSummaryResponse>> ResolveReview(Guid caseId, Guid reviewId, ReviewDecisionRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.ResolveReviewCaseAsync(caseId, reviewId, request, cancellationToken));
}
