using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WAssis.Application.Abstractions;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Services;

public sealed class HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
    public string? TenantId => Principal?.FindFirstValue(ClaimConstants.TenantId);
    public string? BrokerageId => Principal?.FindFirstValue(ClaimConstants.BrokerageId);
    public string? SellerId => Principal?.FindFirstValue(ClaimConstants.SellerId);
    public string? UserType => Principal?.FindFirstValue(ClaimConstants.UserType);
    public IReadOnlyCollection<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(static x => x.Value).ToArray() ?? [];

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
