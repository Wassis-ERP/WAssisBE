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
public sealed class ExecutionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("calculos/{calculationId:guid}/execucoes")]
    [RequirePermission("comercial", "create", supportsOwnRecords: true)]
    [DistributedOperationLimit("executions.create", 30, 60)]
    public async Task<IActionResult> Create(Guid calculationId, CreateCalculationExecution input, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new CreateExecutionCommand(calculationId, key ?? string.Empty, input), token);
        return row is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = row.Id }, row);
    }
    [HttpPost("execucoes/{id:guid}/reexecutar")]
    [RequirePermission("comercial", "create", supportsOwnRecords: true)]
    [DistributedOperationLimit("executions.retry", 30, 60)]
    public async Task<IActionResult> Retry(Guid id, RetryCalculationExecution input, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new RetryExecutionCommand(id, key ?? string.Empty, input), token);
        return row is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = row.Id }, row);
    }
    [HttpPost("execucoes/{id:guid}/cancelar")]
    [RequirePermission("comercial", "update", supportsOwnRecords: true)]
    [DistributedOperationLimit("executions.cancel", 30, 60)]
    public async Task<IActionResult> Cancel(Guid id, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new CancelExecutionCommand(id, key ?? string.Empty), token);
        return row is null ? NotFound() : Ok(row);
    }
    [HttpPost("execucoes/{id:guid}/cotacao-manual")]
    [RequirePermission("comercial", "create", supportsOwnRecords: true)]
    [DistributedOperationLimit("executions.manual-quote", 30, 60)]
    public async Task<IActionResult> RecordQuote(Guid id, RecordManualQuote input, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken token)
    {
        var row = await mediator.Send(new RecordManualQuoteCommand(id, key ?? string.Empty, input), token);
        return row is null ? NotFound() : CreatedAtAction(nameof(GetQuote), new { id = row.Quote.Id }, row);
    }
    [HttpGet("execucoes/{id:guid}")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Get(Guid id, CancellationToken token)
    {
        var row = await mediator.Send(new GetExecutionQuery(id), token); return row is null ? NotFound() : Ok(row);
    }
    [HttpGet("cotacoes/{id:guid}")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> GetQuote(Guid id, CancellationToken token)
    {
        var row = await mediator.Send(new GetQuoteResultQuery(id), token); return row is null ? NotFound() : Ok(row);
    }
    [HttpGet("calculos/{calculationId:guid}/execucoes")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> List(Guid calculationId, int page = 1, int pageSize = 50, CancellationToken token = default)
    {
        var rows = await mediator.Send(new ListExecutionsQuery(calculationId, page, pageSize), token); return rows is null ? NotFound() : Ok(rows);
    }
    [HttpGet("oportunidades/{opportunityId:guid}/cotacoes")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> ListQuotes(Guid opportunityId, int page = 1, int pageSize = 50, CancellationToken token = default)
    {
        var rows = await mediator.Send(new ListQuoteResultsQuery(opportunityId, page, pageSize), token); return rows is null ? NotFound() : Ok(rows);
    }
}
