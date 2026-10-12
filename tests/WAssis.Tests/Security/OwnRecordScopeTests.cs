using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Infra.Data.Context;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Security;

public sealed class OwnRecordScopeTests
{
    [Fact]
    public async Task OwnOpportunityFiltersApplyEvenWhenAuthorizationRunsAfterContextConstruction()
    {
        var database = Guid.NewGuid().ToString();
        var own = OpportunityFor("77777777-7777-7777-7777-777777777777");
        var other = OpportunityFor("77777777-7777-7777-7777-777777777778");
        await using (var seed = Context(database))
        {
            seed.Set<OpportunityStageReference>().Add(new("66666666-6666-6666-6666-666666666666", "55555555-5555-5555-5555-555555555555"));
            seed.Opportunities.AddRange(own, other);
            await seed.SaveChangesAsync();
        }
        var scope = new RecordPermissionScope();
        await using var db = Context(database, scope);
        scope.RestrictToOwner("comercial", "77777777-7777-7777-7777-777777777777", []);
        Assert.Equal(own.Id, (await db.Opportunities.SingleAsync()).Id);
        Assert.Null(await db.Opportunities.SingleOrDefaultAsync(x => x.Id == other.Id));
        var entity = await db.Opportunities.SingleAsync();
        db.Entry(entity).Property(x => x.ResponsibleId).CurrentValue = "77777777-7777-7777-7777-777777777778";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task OwnCustomerRequiresExplicitProducerOrManagerBinding()
    {
        var database = Guid.NewGuid().ToString();
        var byProducer = Customer("88888888-8888-8888-8888-888888888888", null);
        var byManager = Customer(null, "88888888-8888-8888-8888-888888888888");
        var unrelated = Customer("88888888-8888-8888-8888-888888888889", null);
        await using (var seed = Context(database))
        {
            seed.InsuredPeople.AddRange(byProducer, byManager, unrelated);
            await seed.SaveChangesAsync();
        }
        var scope = new RecordPermissionScope();
        scope.RestrictToOwner("segurados", "77777777-7777-7777-7777-777777777777", ["88888888-8888-8888-8888-888888888888"]);
        await using var db = Context(database, scope);
        var ids = await db.InsuredPeople.Select(x => x.Id).ToArrayAsync();
        Assert.Equal(2, ids.Length);
        Assert.Contains(byProducer.Id, ids);
        Assert.Contains(byManager.Id, ids);
        Assert.DoesNotContain(unrelated.Id, ids);
        db.InsuredPeople.Add(Customer("88888888-8888-8888-8888-888888888889", null));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ContextsDoNotReuseAnotherUsersOwnershipScope()
    {
        var database = Guid.NewGuid().ToString();
        await using (var seed = Context(database))
        {
            seed.Set<OpportunityStageReference>().Add(new("66666666-6666-6666-6666-666666666666", "55555555-5555-5555-5555-555555555555"));
            seed.Opportunities.AddRange(OpportunityFor("77777777-7777-7777-7777-777777777777"), OpportunityFor("77777777-7777-7777-7777-777777777778"));
            await seed.SaveChangesAsync();
        }
        foreach (var user in new[] { "77777777-7777-7777-7777-777777777777", "77777777-7777-7777-7777-777777777778" })
        {
            var scope = new RecordPermissionScope();
            scope.RestrictToOwner("comercial", user, []);
            await using var db = Context(database, scope);
            Assert.Equal(user, (await db.Opportunities.SingleAsync()).ResponsibleId);
        }
    }

    private static WAssisDbContext Context(string database, RecordPermissionScope? scope = null) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseInMemoryDatabase(database).Options,
        new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "22222222-2222-2222-2222-222222222222", UserId = "77777777-7777-7777-7777-777777777777", BranchId = "44444444-4444-4444-4444-444444444444", BranchIds = ["44444444-4444-4444-4444-444444444444"] },
        recordScope: scope);

    private static Opportunity OpportunityFor(string responsible) => Opportunity.Create(
        "22222222-2222-2222-2222-222222222222", "44444444-4444-4444-4444-444444444444", "Fixture", responsible, null, null, "66666666-6666-6666-6666-666666666666", null, null, null,
        "pending", null, null, null, null, null, null, null, null, null, null, null);

    private static InsuredPerson Customer(string? producer, string? manager) => InsuredPerson.Create(
        tenantId: "22222222-2222-2222-2222-222222222222", officeBranchId: "44444444-4444-4444-4444-444444444444", name: "Fixture", personType: "PF", status: "Prospecto",
        documentNumber: null, email: null, phoneNumber: null, birthDateUtc: null, tradeName: null, gender: null,
        maritalStatus: null, companySize: null, cnae: null, website: null, postalCode: null, street: null,
        number: null, complement: null, neighborhood: null, city: null, state: null, notes: null,
        producerId: producer, managerId: manager, chatwootId: null, lgpdAuthorized: false, createdBy: "77777777-7777-7777-7777-777777777777");
}
