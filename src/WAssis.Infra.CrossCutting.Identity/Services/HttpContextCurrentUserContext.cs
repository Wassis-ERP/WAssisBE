using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WAssis.Application.Abstractions;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Services;

public sealed class HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
    private string? RequestedBranchId => httpContextAccessor.HttpContext?.Request.Headers["X-Office-Branch-Id"].FirstOrDefault();
    private IReadOnlyCollection<string> ClaimedBranchIds => Principal?
        .FindAll(ClaimConstants.BranchIds)
        .Select(static x => x.Value)
        .Append(Principal?.FindFirstValue(ClaimConstants.BranchId))
        .Where(static x => !string.IsNullOrWhiteSpace(x))
        .Select(static x => x!.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray() ?? [];

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
    public string? TenantId => Principal?.FindFirstValue(ClaimConstants.TenantId);
    public string? BrokerageId => Principal?.FindFirstValue(ClaimConstants.BrokerageId);
    public string? BranchId
    {
        get
        {
            var selected = RequestedBranchId;
            if (!string.IsNullOrWhiteSpace(selected) && ClaimedBranchIds.Contains(selected, StringComparer.OrdinalIgnoreCase))
                return selected.Trim();
            return Principal?.FindFirstValue(ClaimConstants.BranchId);
        }
    }
    public string? SellerId => Principal?.FindFirstValue(ClaimConstants.SellerId);
    public string? UserType => Principal?.FindFirstValue(ClaimConstants.UserType);
    // Acesso de grupo é decidido a cada requisição pela matriz persistida. O
    // contexto de dados fica sempre reduzido à filial selecionada para não
    // confiar em vínculos antigos carregados no token.
    public bool HasAllBranchesAccess => false;
    public IReadOnlyCollection<string> BranchIds => string.IsNullOrWhiteSpace(BranchId) ? [] : [BranchId];
    public IReadOnlyCollection<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(static x => x.Value).ToArray() ?? [];

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
