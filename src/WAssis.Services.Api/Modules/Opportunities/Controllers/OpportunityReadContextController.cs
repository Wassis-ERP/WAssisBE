using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Infrastructure;

namespace WAssis.Services.Api.Modules.Opportunities.Controllers;

[ApiController]
[Route("api/oportunidades/{opportunityId:guid}/contexto-comercial")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class OpportunityReadContextController(IOpportunityReadContext context) : ControllerBase
{
    [HttpGet]
    [RequirePermission("comercial", "read", supportsOwnRecords: true)]
    public async Task<IActionResult> Get(Guid opportunityId, CancellationToken cancellationToken)
    {
        var result = await context.GetAsync(opportunityId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
