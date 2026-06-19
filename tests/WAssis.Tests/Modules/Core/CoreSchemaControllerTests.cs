using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Core.Dtos;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Services.Api.Modules.Core.Controllers;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Core;

public sealed class CoreSchemaControllerTests
{
    [Fact]
    public async Task GetBranches_ShouldRejectIdentityWithoutUuidTenant()
    {
        var repository = new CapturingBranchRepository();
        var controller = new CoreSchemaController(
            new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "invalid" },
            repository);

        var result = await controller.GetBranches(CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.StatusCode);
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task GetBranches_ShouldPassOnlyUuidBranchesFromAuthenticatedScope()
    {
        var tenantId = Guid.NewGuid();
        var firstBranchId = Guid.NewGuid();
        var secondBranchId = Guid.NewGuid();
        var repository = new CapturingBranchRepository();
        var controller = new CoreSchemaController(
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = tenantId.ToString(),
                BranchId = firstBranchId.ToString(),
                BranchIds = [secondBranchId.ToString(), "not-a-uuid", firstBranchId.ToString()],
                HasAllBranchesAccess = false,
            },
            repository);

        var result = await controller.GetBranches(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tenantId, repository.TenantId);
        Assert.False(repository.HasAllBranchesAccess);
        Assert.Equal(2, repository.AllowedBranchIds.Count);
        Assert.Contains(firstBranchId, repository.AllowedBranchIds);
        Assert.Contains(secondBranchId, repository.AllowedBranchIds);
    }

    private sealed class CapturingBranchRepository : ICoreBranchReadRepository
    {
        public bool WasCalled { get; private set; }
        public Guid TenantId { get; private set; }
        public IReadOnlyCollection<Guid> AllowedBranchIds { get; private set; } = [];
        public bool HasAllBranchesAccess { get; private set; }

        public Task<IReadOnlyCollection<CoreBranchDto>> ListAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> allowedBranchIds,
            bool hasAllBranchesAccess,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            TenantId = tenantId;
            AllowedBranchIds = allowedBranchIds;
            HasAllBranchesAccess = hasAllBranchesAccess;
            return Task.FromResult<IReadOnlyCollection<CoreBranchDto>>([]);
        }
    }
}
