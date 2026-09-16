using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Services.Api.Infrastructure;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Security;

public sealed class RequirePermissionAttributeTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid BranchId = Guid.NewGuid();

    [Fact]
    public async Task DeniesByDefaultWhenPersistedPermissionIsAbsent()
    {
        var filter = new RequirePermissionAttribute("financeiro", "read");
        var service = new CapturingPermissionService(false);
        var context = CreateContext(service, BranchId.ToString());

        await filter.OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.Equal(BranchId, service.BranchId);
    }

    [Fact]
    public async Task UsesRequestedBranchAndAllowsOnlyLiveServiceDecision()
    {
        var requestedBranch = Guid.NewGuid();
        var filter = new RequirePermissionAttribute("comercial", "update");
        var service = new CapturingPermissionService(true);
        var context = CreateContext(service, BranchId.ToString());
        context.HttpContext.Request.Headers["X-Office-Branch-Id"] = requestedBranch.ToString();

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal(requestedBranch, service.BranchId);
        Assert.Equal("comercial", service.Module);
        Assert.Equal("update", service.Action);
    }

    [Fact]
    public async Task TenantWideOperationRequiresExplicitGroupScopeDecision()
    {
        var filter = new RequirePermissionAttribute("auditoria", "export", tenantWide: true);
        var service = new CapturingPermissionService(false);
        var context = CreateContext(service, BranchId.ToString());

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<ObjectResult>(context.Result);
        Assert.True(service.TenantWide);
        Assert.Null(service.BranchId);
    }

    private static AuthorizationFilterContext CreateContext(CapturingPermissionService service, string branchId)
    {
        var currentUser = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            TenantId = TenantId.ToString(),
            UserId = UserId.ToString(),
            BranchId = branchId,
            BranchIds = [branchId],
        };
        var services = new ServiceCollection()
            .AddSingleton<ICurrentUserContext>(currentUser)
            .AddSingleton<IEffectivePermissionService>(service)
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }

    private sealed class CapturingPermissionService(bool result) : IEffectivePermissionService
    {
        public Guid? BranchId { get; private set; }
        public string? Module { get; private set; }
        public string? Action { get; private set; }
        public bool TenantWide { get; private set; }

        public Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string module, string action,
            Guid? branchId, bool tenantWide, CancellationToken cancellationToken)
        {
            Assert.Equal(TenantId, tenantId);
            Assert.Equal(UserId, userId);
            BranchId = branchId;
            Module = module;
            Action = action;
            TenantWide = tenantWide;
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<EffectivePermissionDto>> ListAsync(Guid tenantId, Guid userId,
            Guid? branchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<EffectivePermissionDto>>([]);
    }
}
