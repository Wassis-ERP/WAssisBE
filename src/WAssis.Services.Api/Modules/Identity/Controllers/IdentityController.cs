using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Commands;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Extensions;
using WAssis.Services.Api.Infrastructure;
using WAssis.Services.Api.Modules.Identity.Contracts;
using WAssis.Services.Api.Modules.Identity.ViewModels;
using WAssis.Services.Api.Modules.Migration;

namespace WAssis.Services.Api.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(
    ICurrentUserContext currentUserContext,
    IEffectivePermissionService effectivePermissions,
    IAuthSessionService authSessions,
    IHostEnvironment environment,
    IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [DistributedOperationLimit("identity.login", 5, 60)]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Username, request.Password), cancellationToken);
        if (!result.IsSuccess) return this.ToActionResult(result, value => Ok(ToLoginViewModel(value)));
        await authSessions.RegisterAsync(result.Value!.AccessToken, cancellationToken);
        Response.Cookies.Append("wassis_access", result.Value.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(result.Value.ExpiresAtUtc),
            IsEssential = true,
            Path = "/",
        });
        return Ok(ToLoginViewModel(result.Value) with { AccessToken = string.Empty });
    }

    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new CurrentUserViewModel(
            currentUserContext.IsAuthenticated,
            currentUserContext.UserId,
            currentUserContext.TenantId,
            currentUserContext.BrokerageId,
            currentUserContext.BranchId,
            currentUserContext.BranchIds,
            currentUserContext.HasAllBranchesAccess,
            currentUserContext.SellerId,
            currentUserContext.UserType,
            currentUserContext.Roles));
    }

    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpGet("me/permissions")]
    public async Task<IActionResult> Permissions([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserContext.TenantId, out var tenantId)
            || !Guid.TryParse(currentUserContext.UserId, out var userId))
            return Forbid();
        var selected = branchId ?? (Guid.TryParse(currentUserContext.BranchId, out var currentBranch) ? currentBranch : null);
        return Ok(await effectivePermissions.ListAsync(tenantId, userId, selected, cancellationToken));
    }

    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrWhiteSpace(jti)) await authSessions.RevokeAsync(jti, cancellationToken);
        Response.Cookies.Delete("wassis_access", new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
        return NoContent();
    }

    [Authorize(Policy = AccessPolicies.BrokerageAdmin)]
    [HttpPost("users")]
    [RequirePermission("configuracoes", "manage", tenantWide: true)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult CreateUser()
    {
        return MigrationProblemDetailsFactory.Pending(
            this,
            FrontendContractCatalog.IdentityUsers,
            "O cadastro de usuarios internos deve ser implementado no modulo Identity do WAssisBE antes de substituir o fluxo legado.");
    }

    private static LoginResponseViewModel ToLoginViewModel(LoginResultDto result)
    {
        return new LoginResponseViewModel(
            result.AccessToken,
            result.ExpiresAtUtc,
            result.UserId,
            result.TenantId,
            result.BrokerageId,
            result.BranchId,
            result.BranchIds,
            result.HasAllBranchesAccess,
            result.SellerId,
            result.UserType,
            result.Roles);
    }
}
