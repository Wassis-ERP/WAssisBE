using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Policies.Commands;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Application.Modules.Policies.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Extensions;
using WAssis.Services.Api.Modules.Policies.Contracts;
using WAssis.Services.Api.Modules.Policies.ViewModels;

namespace WAssis.Services.Api.Modules.Policies.Controllers;

[ApiController]
[Route("api/policies/drafts")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class PolicyDraftsController(IMediator mediator) : ControllerBase
{
    [HttpPost("from-document")]
    [ProducesResponseType(typeof(PolicyDraftViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFromDocument([FromBody] CreatePolicyDraftFromDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreatePolicyDraftFromDocumentCommand(request.ImportedDocumentId), cancellationToken);
        return this.ToActionResult(result, value =>
            CreatedAtAction(nameof(GetById), new { id = value.Id }, ToViewModel(value)));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyDraftViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPolicyDraftQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/ready")]
    public async Task<IActionResult> MarkReady(Guid id, [FromBody] UpdatePolicyDraftStatusRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MarkPolicyDraftReadyCommand(id, request?.Notes), cancellationToken);
        return this.ToActionResult(result, value => Ok(ToViewModel(value)));
    }

    [HttpPost("{id:guid}/approve-review")]
    public async Task<IActionResult> ApproveReview(Guid id, [FromBody] UpdatePolicyDraftStatusRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ApprovePolicyDraftReviewCommand(id, request?.Notes), cancellationToken);
        return this.ToActionResult(result, value => Ok(ToViewModel(value)));
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, [FromBody] UpdatePolicyDraftStatusRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IssuePolicyDraftCommand(id, request?.PolicyNumber, request?.Notes), cancellationToken);
        return this.ToActionResult(result, value => Ok(ToViewModel(value)));
    }

    private static PolicyDraftViewModel ToViewModel(PolicyDraftDto result)
    {
        return new PolicyDraftViewModel(
            result.Id,
            result.ImportedDocumentId,
            result.CorrelationId,
            result.InsuranceCompanyName,
            result.ProposalNumber,
            result.InsuredName,
            result.CoverageStartDateUtc,
            result.CoverageEndDateUtc,
            result.TotalPremiumAmount,
            result.CommissionAmount,
            result.Status,
            result.PolicyNumber,
            result.Notes,
            result.CreatedAtUtc,
            result.ReviewedAtUtc,
            result.ReviewedByUserId,
            result.ReadyForIssuanceAtUtc,
            result.IssuedAtUtc);
    }
}
