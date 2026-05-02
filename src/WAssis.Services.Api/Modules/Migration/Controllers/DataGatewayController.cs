using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Migration;
using WAssis.Services.Api.Modules.Migration.Contracts;

namespace WAssis.Services.Api.Modules.Migration.Controllers;

[ApiController]
[Route("api/data-gateway")]
[Authorize(Policy = AccessPolicies.AuthenticatedUser)]
public sealed class DataGatewayController : ControllerBase
{
    [HttpGet("/api/migration/frontend-contracts")]
    [ProducesResponseType(typeof(IReadOnlyCollection<FrontendContractGapViewModel>), StatusCodes.Status200OK)]
    public IActionResult FrontendContracts()
    {
        return Ok(FrontendContractCatalog.All);
    }

    [HttpPost("query")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Query([FromBody] DataGatewayQueryRequest request)
    {
        return MigrationProblemDetailsFactory.Pending(
            this,
            FrontendContractCatalog.DataGatewayQuery,
            $"A consulta legada da tabela '{request.Table}' deve ganhar um endpoint dedicado no WAssisBE.");
    }

    [HttpPost("rpc/{name}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Rpc(string name, [FromBody] Dictionary<string, object?> _)
    {
        return MigrationProblemDetailsFactory.Pending(
            this,
            FrontendContractCatalog.DataGatewayRpc,
            $"A RPC legada '{name}' deve ganhar um endpoint dedicado no WAssisBE.");
    }
}
