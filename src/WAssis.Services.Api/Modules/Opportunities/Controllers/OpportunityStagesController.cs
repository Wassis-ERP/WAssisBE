using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;

namespace WAssis.Services.Api.Modules.Opportunities.Controllers;

[ApiController]
[Route("api/oportunidades/stages")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class OpportunityStagesController(IOpportunityScope scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await scope.ListStagesAsync(cancellationToken));
}
