using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Administration.Dtos;
using WAssis.Application.Modules.Administration.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;

namespace WAssis.Services.Api.Modules.Administration.Controllers;

[ApiController]
[Route("api/administration")]
[Authorize(Policy = AccessPolicies.BrokerageAdmin)]
public sealed class AdministrationController(
    ICurrentUserContext currentUser,
    IAdministrationRepository repository,
    IAuditTrailWriter auditTrail) : ControllerBase, IAsyncActionFilter
{
    [NonAction]
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!TryScope(out var tenantId, out var userId))
        {
            context.Result = TenantRequired();
            return;
        }
        var manage = !HttpMethods.IsGet(context.HttpContext.Request.Method);
        if (!await repository.HasAdministrationPermissionAsync(tenantId, userId, manage,
                context.HttpContext.RequestAborted))
        {
            context.Result = Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Permissão administrativa insuficiente.");
            return;
        }
        await next();
    }
    [HttpGet("organization")]
    public async Task<IActionResult> GetOrganization(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        var organization = await repository.GetOrganizationAsync(tenantId, cancellationToken);
        return organization is null ? NotFound() : Ok(organization);
    }

    [HttpPut("organization")]
    public async Task<IActionResult> UpdateOrganization([FromBody] OrganizationUpdateDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (string.IsNullOrWhiteSpace(update.LegalName)) return ValidationProblem("Razão social é obrigatória.");
        if (!ValidEmail(update.Email)) return ValidationProblem("E-mail inválido.");
        if (!ValidUrl(update.Website)) return ValidationProblem("Site deve usar HTTP ou HTTPS.");
        var normalized = update with { DocumentNumber = Digits(update.DocumentNumber) };
        var result = await repository.UpdateOrganizationAsync(tenantId, normalized, cancellationToken);
        if (result is null) return NotFound();
        await AuditAsync("organization.updated", "tenant", tenantId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.GetStatisticsAsync(tenantId, cancellationToken));
    }

    [HttpGet("branches")]
    public async Task<IActionResult> ListBranches(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.ListBranchesAsync(tenantId, cancellationToken));
    }

    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranch([FromBody] AdministrationBranchWriteDto create, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        var validation = ValidateBranch(create);
        if (validation is not null) return validation;
        AdministrationBranchDto branch;
        try
        {
            branch = await repository.CreateBranchAsync(tenantId, Normalize(create), cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BusinessConflict(exception.Message);
        }
        await AuditAsync("branch.created", "filial", branch.Id, cancellationToken);
        return CreatedAtAction(nameof(ListBranches), branch);
    }

    [HttpPut("branches/{branchId:guid}")]
    public async Task<IActionResult> UpdateBranch(Guid branchId, [FromBody] AdministrationBranchWriteDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        var validation = ValidateBranch(update, branchId);
        if (validation is not null) return validation;
        AdministrationBranchDto? branch;
        try
        {
            branch = await repository.UpdateBranchAsync(tenantId, branchId, Normalize(update), cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BusinessConflict(exception.Message);
        }
        if (branch is null) return NotFound();
        await AuditAsync(update.IsActive ? "branch.updated" : "branch.deactivated", "filial", branch.Id, cancellationToken);
        return Ok(branch);
    }

    [HttpGet("users")]
    public async Task<IActionResult> ListUsers(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.ListUsersAsync(tenantId, cancellationToken));
    }

    [HttpPost("users")]
    public async Task<IActionResult> InviteUser([FromBody] UserInvitationDto invitation, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (string.IsNullOrWhiteSpace(invitation.Name) || !ValidEmail(invitation.Email))
            return ValidationProblem("Nome e e-mail válidos são obrigatórios.");
        var user = await repository.InviteUserAsync(tenantId, invitation, cancellationToken);
        await AuditAsync("user.added.pending", "profile", user.Id, cancellationToken);
        return CreatedAtAction(nameof(ListUsers), user);
    }

    [HttpPatch("users/{userId:guid}/status")]
    public async Task<IActionResult> SetUserStatus(Guid userId, [FromBody] UserStatusUpdateDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out var actorUserId)) return TenantRequired();
        try
        {
            if (!await repository.SetUserStatusAsync(tenantId, userId, update.IsActive, actorUserId, cancellationToken))
                return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BusinessConflict(exception.Message);
        }
        await AuditAsync(update.IsActive ? "user.activated" : "user.deactivated", "profile", userId, cancellationToken);
        return Ok(new { userId, isActive = update.IsActive });
    }

    [HttpGet("users/{userId:guid}/branches")]
    public async Task<IActionResult> ListUserBranches(Guid userId, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.ListUserBranchAccessAsync(tenantId, userId, cancellationToken));
    }

    [HttpPut("users/{userId:guid}/branches/{branchId:guid}")]
    public async Task<IActionResult> SetUserBranch(
        Guid userId, Guid branchId, [FromBody] UserBranchAccessUpdateDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (update.StartsOn.HasValue && update.EndsOn.HasValue && update.StartsOn > update.EndsOn)
            return ValidationProblem("A data inicial não pode ser posterior à data final.");
        UserBranchAccessDto? result;
        try
        {
            result = await repository.UpsertUserBranchAccessAsync(tenantId, userId, branchId, update, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BusinessConflict(exception.Message);
        }
        if (result is null) return NotFound();
        await AuditAsync("user.branch-access.updated", "profile_filial", result.Id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("access-profiles")]
    public async Task<IActionResult> ListAccessProfiles(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.ListAccessProfilesAsync(tenantId, cancellationToken));
    }

    [HttpPost("access-profiles")]
    public async Task<IActionResult> CreateAccessProfile([FromBody] AccessProfileCreateDto create, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (string.IsNullOrWhiteSpace(create.Name)) return ValidationProblem("Nome do perfil é obrigatório.");
        var result = await repository.CreateAccessProfileAsync(tenantId, create, cancellationToken);
        await AuditAsync("access-profile.created", "perfil", result.Id, cancellationToken);
        return CreatedAtAction(nameof(ListAccessProfiles), result);
    }

    [HttpPut("access-profiles/{profileId:guid}")]
    public async Task<IActionResult> UpdateAccessProfile(Guid profileId, [FromBody] AccessProfileUpdateDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (string.IsNullOrWhiteSpace(update.Name)) return ValidationProblem("Nome do perfil é obrigatório.");
        var result = await repository.UpdateAccessProfileAsync(tenantId, profileId, update, cancellationToken);
        if (result is null) return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Perfil não pode ser alterado ou não existe." });
        await AuditAsync("access-profile.updated", "perfil", result.Id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> ListPermissions(CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        return Ok(await repository.ListPermissionsAsync(tenantId, cancellationToken));
    }

    [HttpPut("permissions/{permissionId:guid}")]
    public async Task<IActionResult> UpdatePermission(Guid permissionId, [FromBody] AccessPermissionUpdateDto update, CancellationToken cancellationToken)
    {
        if (!TryScope(out var tenantId, out _)) return TenantRequired();
        if (update.Scope is not ("GRUPO" or "CORRETORA" or "PROPRIO"))
            return ValidationProblem("Escopo deve ser GRUPO, CORRETORA ou PROPRIO.");
        var result = await repository.UpdatePermissionAsync(tenantId, permissionId, update, cancellationToken);
        if (result is null) return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Permissão protegida ou inexistente." });
        await AuditAsync("permission.updated", "role_permission", result.Id, cancellationToken);
        return Ok(result);
    }

    private bool TryScope(out Guid tenantId, out Guid userId)
    {
        var hasTenant = Guid.TryParse(currentUser.TenantId, out tenantId);
        var hasUser = Guid.TryParse(currentUser.UserId, out userId);
        return hasTenant && hasUser;
    }

    private ObjectResult TenantRequired() => Problem(
        statusCode: StatusCodes.Status403Forbidden,
        title: "Contexto administrativo inválido.",
        detail: "Tenant e usuário UUID são obrigatórios para administrar a organização.");

    private BadRequestObjectResult ValidationProblem(string detail) => BadRequest(new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Dados inválidos.",
        Detail = detail,
    });

    private ConflictObjectResult BusinessConflict(string detail) => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Operação não permitida.",
        Detail = detail,
    });

    private Task AuditAsync(string action, string entityType, Guid entityId, CancellationToken cancellationToken) =>
        auditTrail.WriteAsync(HttpContext.TraceIdentifier, "administration", action, entityType, entityId.ToString(), null, cancellationToken);

    private static string? Digits(string? value) => string.IsNullOrWhiteSpace(value)
        ? null : new string(value.Where(char.IsDigit).ToArray());

    private static bool ValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        try { return new MailAddress(value).Address == value.Trim(); }
        catch (FormatException) { return false; }
    }

    private static bool ValidUrl(string? value) => string.IsNullOrWhiteSpace(value)
        || Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";

    private BadRequestObjectResult? ValidateBranch(AdministrationBranchWriteDto branch, Guid? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(branch.LegalName)) return ValidationProblem("Razão social da corretora é obrigatória.");
        if (branch.ParentBranchId == branchId) return ValidationProblem("Uma corretora não pode ser matriz de si mesma.");
        if (!ValidEmail(branch.Email)) return ValidationProblem("E-mail da corretora inválido.");
        if (!ValidUrl(branch.Website)) return ValidationProblem("Site da corretora deve usar HTTP ou HTTPS.");
        if (branch.TaxPercentage is < 0 or > 100 || branch.IssPercentage is < 0 or > 100)
            return ValidationProblem("Percentuais devem ficar entre 0 e 100.");
        return null;
    }

    private static AdministrationBranchWriteDto Normalize(AdministrationBranchWriteDto branch) => branch with
    {
        DocumentNumber = Digits(branch.DocumentNumber),
        PostalCode = Digits(branch.PostalCode),
        State = branch.State?.Trim().ToUpperInvariant()[..Math.Min(branch.State.Trim().Length, 2)],
    };
}
