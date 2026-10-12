using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Infrastructure;

namespace WAssis.Services.Api.Modules.Calculations;

[ApiController]
[Route("api")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class PresentationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("oportunidades/{opportunityId:guid}/apresentacoes")]
    [RequirePermission("comercial", "create", supportsOwnRecords: true)]
    [DistributedOperationLimit("presentations.create", 30, 60)]
    public async Task<IActionResult> Create(Guid opportunityId, SaveCommercialPresentation input, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new SavePresentationCommand(opportunityId, false, key ?? string.Empty, input), token);
        return row is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = row.Presentation.Id }, row);
    }
    [HttpPut("apresentacoes/{id:guid}")]
    [RequirePermission("comercial", "update", supportsOwnRecords: true)]
    [DistributedOperationLimit("presentations.update", 30, 60)]
    public async Task<IActionResult> Update(Guid id, SaveCommercialPresentation input, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new SavePresentationCommand(id, true, key ?? string.Empty, input), token);
        return row is null ? NotFound() : Ok(row);
    }
    [HttpGet("apresentacoes/{id:guid}")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Get(Guid id, CancellationToken token)
    {
        var row = await mediator.Send(new GetPresentationQuery(id), token); return row is null ? NotFound() : Ok(row);
    }
    [HttpGet("oportunidades/{opportunityId:guid}/apresentacoes")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> List(Guid opportunityId, int page = 1, int pageSize = 50, CancellationToken token = default)
    {
        var rows = await mediator.Send(new ListPresentationsQuery(opportunityId, page, pageSize), token); return rows is null ? NotFound() : Ok(rows);
    }
}
