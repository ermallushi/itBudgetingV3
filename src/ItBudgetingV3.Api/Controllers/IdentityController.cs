using ItBudgetingV3.Application.Contracts;
using ItBudgetingV3.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV3.Api.Controllers;

[ApiController]
[Route("api/cases/{caseId:guid}/identity")]
public sealed class IdentityController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost("profile")]
    public async Task<ActionResult<CaseSummaryResponse>> SubmitProfile(Guid caseId, PersonProfileRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.SubmitPersonProfileAsync(caseId, request, cancellationToken));

    [HttpPost("documents")]
    public async Task<ActionResult<CaseSummaryResponse>> SubmitDocument(Guid caseId, IdentityDocumentRequest request, CancellationToken cancellationToken)
        => Ok(await onboardingService.SubmitIdentityDocumentAsync(caseId, request, cancellationToken));
}
