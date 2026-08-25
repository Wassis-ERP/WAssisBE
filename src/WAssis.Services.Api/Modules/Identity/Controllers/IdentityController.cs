using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Commands;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Extensions;
using WAssis.Services.Api.Modules.Identity.Contracts;
using WAssis.Services.Api.Modules.Identity.ViewModels;
using WAssis.Services.Api.Modules.Migration;

namespace WAssis.Services.Api.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(ICurrentUserContext currentUserContext, IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Username, request.Password), cancellationToken);
        return this.ToActionResult(result, value => Ok(ToLoginViewModel(value)));
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

    [Authorize(Policy = AccessPolicies.BrokerageAdmin)]
    [HttpPost("users")]
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
