using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Services.Api.Modules.Core.Controllers;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Core;

public sealed class CoreCatalogsControllerTests
{
    [Fact]
    public async Task List_ShouldPassAuthenticatedTenantAndBranchScope()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var repository = new CapturingCatalogRepository();
        var controller = new CoreCatalogsController(
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = tenantId.ToString(),
                BranchId = branchId.ToString(),
                BranchIds = [branchId.ToString()],
            },
            repository);

        var result = await controller.List("ramos", true, "auto", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tenantId, repository.TenantId);
        Assert.Contains(branchId, repository.AllowedBranchIds);
        Assert.Equal("auto", repository.Search);
    }

    [Fact]
    public async Task List_ShouldRejectUnknownResourceBeforeRepositoryCall()
    {
        var repository = new CapturingCatalogRepository();
        var controller = new CoreCatalogsController(
            new FakeCurrentUserContext { IsAuthenticated = true, TenantId = Guid.NewGuid().ToString() },
            repository);

        var result = await controller.List("unknown", true, null, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task List_ShouldRejectSearchLongerThanOneHundredCharacters()
    {
        var repository = new CapturingCatalogRepository();
        var controller = new CoreCatalogsController(
            new FakeCurrentUserContext { IsAuthenticated = true, TenantId = Guid.NewGuid().ToString() },
            repository);

        var result = await controller.List("ramos", true, new string('a', 101), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(repository.WasCalled);
    }

    private sealed class CapturingCatalogRepository : ICoreCatalogReadRepository
    {
        public IReadOnlyCollection<string> SupportedResources => ["ramos"];
        public bool WasCalled { get; private set; }
        public Guid TenantId { get; private set; }
        public IReadOnlyCollection<Guid> AllowedBranchIds { get; private set; } = [];
        public string? Search { get; private set; }

        public Task<IReadOnlyCollection<string>> ListJsonAsync(
            string resource,
            Guid tenantId,
            IReadOnlyCollection<Guid> allowedBranchIds,
            bool hasAllBranchesAccess,
            bool activeOnly,
            string? search,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            TenantId = tenantId;
            AllowedBranchIds = allowedBranchIds;
            Search = search;
            return Task.FromResult<IReadOnlyCollection<string>>(["{\"id\":\"test\"}"]);
        }
    }
}
