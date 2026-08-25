using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;

namespace WAssis.Services.Api.Modules.Core.Controllers;

[ApiController]
[Route("api/core/catalogs")]
[Authorize(Policy = AccessPolicies.BrokerageAdmin)]
public sealed class CoreCatalogsController(
    ICurrentUserContext currentUserContext,
    ICoreCatalogReadRepository repository) : ControllerBase
{
    [HttpGet]
    public IActionResult GetSupportedResources()
    {
        return Ok(repository.SupportedResources);
    }

    [HttpGet("{resource}")]
    public async Task<IActionResult> List(
        string resource,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (!repository.SupportedResources.Contains(resource, StringComparer.OrdinalIgnoreCase))
        {
            return NotFound(new { message = "Catalogo nao suportado.", supported = repository.SupportedResources });
        }

        if (search?.Length > 100)
        {
            return BadRequest(new { message = "A busca deve ter no maximo 100 caracteres." });
        }

        if (!Guid.TryParse(currentUserContext.TenantId, out var tenantId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Valid tenant context is required.");
        }

        var branchIds = currentUserContext.BranchIds
            .Append(currentUserContext.BranchId)
            .Where(static value => Guid.TryParse(value, out _))
            .Select(static value => Guid.Parse(value!))
            .Distinct()
            .ToArray();

        var rows = await repository.ListJsonAsync(
            resource,
            tenantId,
            branchIds,
            currentUserContext.HasAllBranchesAccess,
            activeOnly,
            search,
            cancellationToken);

        return Ok(rows.Select(ParseJson));
    }

    private static JsonElement ParseJson(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }
}
