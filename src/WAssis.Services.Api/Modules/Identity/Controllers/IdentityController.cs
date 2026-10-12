using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Commands;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.CrossCutting.Identity.Models;
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
    IExternalIdentityResolver externalIdentities,
    IInternalAccessTokenIssuer internalTokens,
    IOptions<Auth0Options> auth0Options,
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
        try { await authSessions.RegisterAsync(result.Value!.AccessToken, cancellationToken); }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não foi possível autenticar com as credenciais informadas.",
            });
        }
        AppendAccessCookie(result.Value);
        return Ok(ToLoginViewModel(result.Value) with { AccessToken = string.Empty });
    }

    [AllowAnonymous]
    [HttpGet("external/login")]
    public IActionResult ExternalLogin()
    {
        if (!auth0Options.Value.Enabled) return NotFound();
        return Challenge(
            new AuthenticationProperties { RedirectUri = "/api/identity/external/complete" },
            ExternalAuthenticationSchemes.Auth0);
    }

    [AllowAnonymous]
    [HttpGet("external/complete")]
    public async Task<IActionResult> ExternalComplete(CancellationToken cancellationToken)
    {
        if (!auth0Options.Value.Enabled) return NotFound();
        var authentication = await HttpContext.AuthenticateAsync(ExternalAuthenticationSchemes.TemporaryCookie);
        var principal = authentication.Principal;
        var subject = principal?.FindFirst("sub")?.Value;
        if (!authentication.Succeeded || string.IsNullOrWhiteSpace(subject))
            return Unauthorized(new ProblemDetails { Status = 401, Title = "A autenticação corporativa não pôde ser concluída." });

        var identity = await externalIdentities.ResolveAsync(auth0Options.Value.Authority, subject, cancellationToken);
        if (identity is null)
        {
            await HttpContext.SignOutAsync(ExternalAuthenticationSchemes.TemporaryCookie);
            return Forbid();
        }

        var mfaSatisfied = principal!.FindAll("amr").Any(claim =>
            claim.Value.Split([' ', ',', '[', ']', '"'], StringSplitOptions.RemoveEmptyEntries)
                .Contains("mfa", StringComparer.OrdinalIgnoreCase));
        if (identity.RequiresMfa && !mfaSatisfied)
        {
            await HttpContext.SignOutAsync(ExternalAuthenticationSchemes.TemporaryCookie);
            return Forbid();
        }

        var session = internalTokens.Issue(identity);
        await authSessions.RegisterAsync(session.AccessToken, cancellationToken);
        AppendAccessCookie(session);
        await HttpContext.SignOutAsync(ExternalAuthenticationSchemes.TemporaryCookie);
        return Redirect(auth0Options.Value.FrontendCallbackUrl);
    }

    [AllowAnonymous]
    [HttpGet("external/logout")]
    public async Task<IActionResult> ExternalLogout(CancellationToken cancellationToken)
    {
        if (!auth0Options.Value.Enabled) return NotFound();
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrWhiteSpace(jti)) await authSessions.RevokeAsync(jti, cancellationToken);
        DeleteAccessCookie();
        return SignOut(
            new AuthenticationProperties { RedirectUri = auth0Options.Value.FrontendLogoutUrl },
            ExternalAuthenticationSchemes.Auth0);
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
    [HttpGet("me/branches")]
    [ProducesResponseType(typeof(IReadOnlyCollection<IdentityBranchViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyBranches(
        [FromServices] ICoreBranchReadRepository branchesRepository,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserContext.TenantId, out var tenantId)) return Forbid();
        // Authentication replaces these claims with the currently active persisted bindings.
        // This is identity context, not an administrative tenant-wide directory.
        // ICurrentUserContext.BranchIds deliberately contains only the selected data branch.
        // Use the principal claims refreshed by OnTokenValidated, without widening that scope.
        var allowed = User.FindAll(ClaimConstants.BranchIds).Select(claim => claim.Value)
            .Where(value => Guid.TryParse(value, out _)).Select(Guid.Parse).Distinct().ToArray();
        var branches = await branchesRepository.ListAsync(tenantId, allowed, false, cancellationToken);
        return Ok(branches.Where(branch => branch.IsActive)
            .Select(branch => new IdentityBranchViewModel(branch.Id, branch.Name, branch.IsActive)).ToArray());
    }

    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpGet("me/permissions")]
    public async Task<IActionResult> Permissions([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserContext.TenantId, out var tenantId)
            || !Guid.TryParse(currentUserContext.UserId, out var userId))
            return Forbid();
        // Omitted branch means all current bindings for this user, not just the
        // branch from the JWT/header. This list drives the CRM branch selector.
        return Ok(await effectivePermissions.ListAsync(tenantId, userId, branchId, cancellationToken));
    }

    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrWhiteSpace(jti)) await authSessions.RevokeAsync(jti, cancellationToken);
        DeleteAccessCookie();
        return NoContent();
    }

    private void DeleteAccessCookie()
    {
        Response.Cookies.Delete("wassis_access", new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
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

    private void AppendAccessCookie(LoginResultDto result)
    {
        Response.Cookies.Append("wassis_access", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(result.ExpiresAtUtc),
            IsEssential = true,
            Path = "/",
        });
    }
}
