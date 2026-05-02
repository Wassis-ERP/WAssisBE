using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Migration;

namespace WAssis.Services.Api.Modules.Migration.Controllers;

[ApiController]
[Authorize(Policy = AccessPolicies.AuthenticatedUser)]
public sealed class LegacyIntegrationController : ControllerBase
{
    [HttpPost("api/storage/{bucket}/{action}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Storage(string bucket, string action, [FromBody] object? _)
    {
        return MigrationProblemDetailsFactory.Pending(
            this,
            FrontendContractCatalog.LegacyStorage,
            $"A chamada de storage '{bucket}/{action}' deve ser substituida por um endpoint de documentos/arquivos no WAssisBE.");
    }

    [HttpPost("api/functions/{name}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Function(string name, [FromBody] object? _)
    {
        return MigrationProblemDetailsFactory.Pending(
            this,
            FrontendContractCatalog.LegacyFunction,
            $"A funcao legada '{name}' deve ser substituida por um endpoint ou worker do WAssisBE.");
    }
}
