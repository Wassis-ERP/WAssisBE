using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Abstractions;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Identity.ViewModels;

namespace WAssis.Services.Api.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(ICurrentUserContext currentUserContext) : ControllerBase
{
    [Authorize(Policy = AccessPolicies.AuthenticatedUser)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new CurrentUserViewModel(
            currentUserContext.IsAuthenticated,
            currentUserContext.UserId,
            currentUserContext.TenantId,
            currentUserContext.BrokerageId,
            currentUserContext.SellerId,
            currentUserContext.UserType,
            currentUserContext.Roles));
    }
}
