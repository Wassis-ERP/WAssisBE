using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Interfaces;

namespace WAssis.Services.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute(string module, string action, bool tenantWide = false)
    : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserContext>();
        var service = context.HttpContext.RequestServices.GetRequiredService<IEffectivePermissionService>();
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenantId)
            || !Guid.TryParse(user.UserId, out var userId))
        {
            context.Result = Forbidden("permission_context_invalid", "Contexto de autorização inválido.");
            return;
        }

        var branchId = tenantWide ? null : ResolveBranch(context, user);
        if (!tenantWide && branchId is null)
        {
            context.Result = Forbidden("branch_context_required", "Uma corretora válida deve ser selecionada.");
            return;
        }

        if (!await service.HasPermissionAsync(
                tenantId, userId, module, action, branchId, tenantWide, context.HttpContext.RequestAborted))
        {
            context.Result = Forbidden("permission_denied", "Permissão insuficiente para esta operação.");
        }
    }

    private static Guid? ResolveBranch(AuthorizationFilterContext context, ICurrentUserContext user)
    {
        var request = context.HttpContext.Request;
        var candidates = new List<object?>
        {
            request.Headers["X-Office-Branch-Id"].FirstOrDefault(),
            request.Query["officeBranchId"].FirstOrDefault(),
            request.Query["branchId"].FirstOrDefault(),
            context.RouteData.Values.GetValueOrDefault("branchId"),
        };
        candidates.Add(user.BranchId);
        return candidates.Select(static value => value?.ToString()).Select(Parse).FirstOrDefault(static value => value.HasValue);
    }

    private static Guid? Parse(string? value) => Guid.TryParse(value, out var parsed) ? parsed : null;

    private static ObjectResult Forbidden(string code, string detail) => new(new ProblemDetails
    {
        Status = StatusCodes.Status403Forbidden,
        Title = "Acesso negado.",
        Detail = detail,
        Extensions = { ["code"] = code },
    }) { StatusCode = StatusCodes.Status403Forbidden };
}
