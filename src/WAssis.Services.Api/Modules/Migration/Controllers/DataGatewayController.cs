using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Migration.Contracts;

namespace WAssis.Services.Api.Modules.Migration.Controllers;

[ApiController]
[Route("api/data-gateway")]
[Authorize(Policy = AccessPolicies.AuthenticatedUser)]
public sealed class DataGatewayController : ControllerBase
{
    [HttpPost("query")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Query([FromBody] DataGatewayQueryRequest request)
    {
        return MigrationPending(
            $"A consulta legada da tabela '{request.Table}' deve ganhar um endpoint dedicado no WAssisBE.");
    }

    [HttpPost("rpc/{name}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Rpc(string name, [FromBody] Dictionary<string, object?> _)
    {
        return MigrationPending(
            $"A RPC legada '{name}' deve ganhar um endpoint dedicado no WAssisBE.");
    }

    private ObjectResult MigrationPending(string detail)
    {
        return Problem(
            title: "Contrato em migracao para o WAssisBE",
            detail: detail,
            statusCode: StatusCodes.Status501NotImplemented);
    }
}
