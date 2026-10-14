using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Infrastructure;

namespace WAssis.Services.Api.Modules.Calculations;

[ApiController]
[Route("api/oportunidades/{opportunityId:guid}/calculos")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class CalculationLookupsController(IMediator mediator) : ControllerBase
{
    [HttpGet("lookups/seguradoras")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Insurers(Guid opportunityId, int page = 1, int pageSize = 50, string? search = null, bool includeReferencedInactive = false, CancellationToken token = default)
    {
        var rows = await mediator.Send(new ListCalculationInsurersQuery(opportunityId, page, pageSize, search, includeReferencedInactive), token); return rows is null ? NotFound() : Ok(rows);
    }
    [HttpGet("lookups/coberturas")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Coverages(Guid opportunityId, [FromQuery] Guid ramoId, int page = 1, int pageSize = 50, string? search = null, CancellationToken token = default)
    {
        var rows = await mediator.Send(new ListCalculationCoveragesQuery(opportunityId, ramoId, page, pageSize, search), token); return rows is null ? NotFound() : Ok(rows);
    }
    [HttpGet("contexto")]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Context(Guid opportunityId, [FromQuery] Guid? ramoId = null, CancellationToken token = default)
    {
        var context = await mediator.Send(new GetCalculationContextQuery(opportunityId, ramoId), token); return context is null ? NotFound() : Ok(context);
    }
}
