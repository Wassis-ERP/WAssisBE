using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WAssis.Services.Api.Modules.Identity.Controllers;
using WAssis.Services.Api.Modules.Migration;
using WAssis.Services.Api.Modules.Migration.Contracts;
using WAssis.Services.Api.Modules.Migration.Controllers;

namespace WAssis.Tests.Modules.Migration;

public sealed class FrontendContractMigrationTests
{
    [Fact]
    public void FrontendContractCatalog_ListsEveryTemporaryFrontendContract()
    {
        var keys = FrontendContractCatalog.All.Select(static x => x.ContractKey).ToArray();

        Assert.Contains("data-gateway.query", keys);
        Assert.Contains("data-gateway.rpc", keys);
        Assert.Contains("legacy.storage", keys);
        Assert.Contains("legacy.function", keys);
        Assert.Contains("identity.users.create", keys);
        Assert.All(FrontendContractCatalog.All, gap =>
        {
            Assert.False(string.IsNullOrWhiteSpace(gap.OwnerIssue));
            Assert.False(string.IsNullOrWhiteSpace(gap.NextStep));
        });
    }

    [Fact]
    public void Query_ReturnsTrackableProblemDetails()
    {
        var controller = new DataGatewayController();

        var result = Assert.IsType<ObjectResult>(controller.Query(new DataGatewayQueryRequest(
            "oportunidades",
            "select",
            "*",
            [],
            new Dictionary<string, object?>(),
            null,
            false)));

        AssertMigrationProblem(result, "data-gateway.query", "Wassis-ERP/WAssisBE#6");
    }

    [Fact]
    public void Rpc_ReturnsTrackableProblemDetails()
    {
        var controller = new DataGatewayController();

        var result = Assert.IsType<ObjectResult>(controller.Rpc(
            "get_team_members",
            new Dictionary<string, object?>()));

        AssertMigrationProblem(result, "data-gateway.rpc", "Wassis-ERP/WAssisBE#6");
    }

    [Fact]
    public void Storage_ReturnsTrackableProblemDetails()
    {
        var controller = new LegacyIntegrationController();

        var result = Assert.IsType<ObjectResult>(controller.Storage("documents", "upload", new { }));

        AssertMigrationProblem(result, "legacy.storage", "Wassis-ERP/WAssisBE#8");
    }

    [Fact]
    public void Function_ReturnsTrackableProblemDetails()
    {
        var controller = new LegacyIntegrationController();

        var result = Assert.IsType<ObjectResult>(controller.Function("invite-user", new { }));

        AssertMigrationProblem(result, "legacy.function", "Wassis-ERP/WAssisBE#8");
    }

    [Fact]
    public void CreateUser_ReturnsTrackableProblemDetails()
    {
        var controller = new IdentityController(new NullCurrentUserContext(), mediator: null!);

        var result = Assert.IsType<ObjectResult>(controller.CreateUser());

        AssertMigrationProblem(result, "identity.users.create", "Wassis-ERP/WAssisBE#8");
    }

    [Fact]
    public void FrontendContracts_ReturnsCatalog()
    {
        var controller = new DataGatewayController();

        var result = Assert.IsType<OkObjectResult>(controller.FrontendContracts());
        var contracts = Assert.IsAssignableFrom<IReadOnlyCollection<FrontendContractGapViewModel>>(result.Value);

        Assert.Equal(FrontendContractCatalog.All.Count, contracts.Count);
    }

    private static void AssertMigrationProblem(ObjectResult result, string contractKey, string ownerIssue)
    {
        Assert.Equal(StatusCodes.Status501NotImplemented, result.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status501NotImplemented, problem.Status);
        Assert.Equal("frontend_contract.pending", problem.Extensions["code"]);
        Assert.Equal(contractKey, problem.Extensions["contractKey"]);
        Assert.Equal(ownerIssue, problem.Extensions["ownerIssue"]);
        Assert.True(problem.Extensions.ContainsKey("nextStep"));
    }

    private sealed class NullCurrentUserContext : WAssis.Application.Abstractions.ICurrentUserContext
    {
        public bool IsAuthenticated => false;
        public string? UserId => null;
        public string? TenantId => null;
        public string? BrokerageId => null;
        public string? SellerId => null;
        public string? UserType => null;
        public IReadOnlyCollection<string> Roles => [];

        public bool IsInRole(string role) => false;
    }
}
