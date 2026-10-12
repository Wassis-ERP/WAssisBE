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
public sealed class CalculationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("oportunidades/{opportunityId:guid}/calculos")]
    [RequirePermission("comercial", "create", supportsOwnRecords: true)]
    [DistributedOperationLimit("calculations.create", 10, 60)]
    public async Task<IActionResult> Create(Guid opportunityId, [FromBody] CreateAutoCalculation input,
        [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCalculationCommand(opportunityId, key ?? string.Empty, input), cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = result.Calculation.Id }, result);
    }

    [HttpGet("calculos/{id:guid}")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCalculationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("oportunidades/{opportunityId:guid}/calculos")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> List(Guid opportunityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListCalculationsQuery(opportunityId, page, pageSize), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
