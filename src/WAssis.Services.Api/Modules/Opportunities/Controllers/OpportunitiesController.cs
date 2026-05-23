using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Opportunities.Commands;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Opportunities.Contracts;
using WAssis.Services.Api.Modules.Opportunities.ViewModels;

namespace WAssis.Services.Api.Modules.Opportunities.Controllers;

[ApiController]
[Route("api/oportunidades")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class OpportunitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<OpportunityViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? pipelineId,
        [FromQuery] string? stageId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new ListOpportunitiesQuery(pipelineId, stageId, status), cancellationToken);
        return Ok(response.Select(ToViewModel).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OpportunityViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetOpportunityQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OpportunityViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateOpportunityCommand(
            request.OfficeBranchId,
            request.Name,
            request.ResponsibleId,
            request.InsuredPersonId,
            request.PipelineId,
            request.StageId,
            request.InsuranceLineId,
            request.InsurerId,
            request.OriginId,
            request.Status,
            request.BusinessType,
            request.ContactType,
            request.NetPremium,
            request.CommissionPercentage,
            request.AgencyPercentage,
            request.ValidityStartUtc,
            request.ValidityEndUtc,
            request.NextFollowUpUtc,
            request.Referrer,
            request.Notes,
            ToMetadataJson(request.Metadata)), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ToViewModel(response));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OpportunityViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new UpdateOpportunityCommand(
            id,
            request.OfficeBranchId,
            request.Name,
            request.ResponsibleId,
            request.InsuredPersonId,
            request.PipelineId,
            request.StageId,
            request.InsuranceLineId,
            request.InsurerId,
            request.OriginId,
            request.LossReasonId,
            request.Status,
            request.BusinessType,
            request.ContactType,
            request.NetPremium,
            request.CommissionPercentage,
            request.AgencyPercentage,
            request.ProductionAmount,
            request.ValidityStartUtc,
            request.ValidityEndUtc,
            request.NextFollowUpUtc,
            request.ConcludedAtUtc,
            request.Referrer,
            request.Notes,
            ToMetadataJson(request.Metadata)), cancellationToken);

        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    [HttpPatch("{id:guid}/stage")]
    [ProducesResponseType(typeof(OpportunityViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveStage(Guid id, [FromBody] MoveOpportunityStageRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new MoveOpportunityStageCommand(id, request.StageId), cancellationToken);
        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    private static OpportunityViewModel ToViewModel(OpportunityDto response)
    {
        return new OpportunityViewModel(
            response.Id,
            response.OfficeBranchId,
            response.Name,
            response.ResponsibleId,
            response.InsuredPersonId,
            response.PipelineId,
            response.StageId,
            response.InsuranceLineId,
            response.InsurerId,
            response.OriginId,
            response.LossReasonId,
            response.Status,
            response.BusinessType,
            response.ContactType,
            response.NetPremium,
            response.CommissionPercentage,
            response.AgencyPercentage,
            response.ProductionAmount,
            response.ValidityStartUtc,
            response.ValidityEndUtc,
            response.NextFollowUpUtc,
            response.ConcludedAtUtc,
            response.Referrer,
            response.Notes,
            ParseMetadata(response.MetadataJson),
            response.CreatedAtUtc,
            response.UpdatedAtUtc);
    }

    private static string ToMetadataJson(JsonElement? metadata)
    {
        if (metadata is null || metadata.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return "{}";
        }

        return metadata.Value.GetRawText();
    }

    private static JsonElement ParseMetadata(string metadataJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson);
        return document.RootElement.Clone();
    }
}
