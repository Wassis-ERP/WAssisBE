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
        var own = OpportunityFor("me");
        var other = OpportunityFor("someone-else");
        await using (var seed = Context(database))
        {
            seed.Opportunities.AddRange(own, other);
            await seed.SaveChangesAsync();
        }
        var scope = new RecordPermissionScope();
        await using var db = Context(database, scope);
        scope.RestrictToOwner("comercial", "me", []);
        Assert.Equal(own.Id, (await db.Opportunities.SingleAsync()).Id);
        Assert.Null(await db.Opportunities.SingleOrDefaultAsync(x => x.Id == other.Id));
        var entity = await db.Opportunities.SingleAsync();
        db.Entry(entity).Property(x => x.ResponsibleId).CurrentValue = "someone-else";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task OwnCustomerRequiresExplicitProducerOrManagerBinding()
    {
        var database = Guid.NewGuid().ToString();
        var byProducer = Customer("producer-me", null);
        var byManager = Customer(null, "producer-me");
        var unrelated = Customer("external-producer", null);
        await using (var seed = Context(database))
        {
            seed.InsuredPeople.AddRange(byProducer, byManager, unrelated);
            await seed.SaveChangesAsync();
        }
        var scope = new RecordPermissionScope();
        scope.RestrictToOwner("segurados", "me", ["producer-me"]);
        await using var db = Context(database, scope);
        var ids = await db.InsuredPeople.Select(x => x.Id).ToArrayAsync();
        Assert.Equal(2, ids.Length);
        Assert.Contains(byProducer.Id, ids);
        Assert.Contains(byManager.Id, ids);
        Assert.DoesNotContain(unrelated.Id, ids);
        db.InsuredPeople.Add(Customer("external-producer", null));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ContextsDoNotReuseAnotherUsersOwnershipScope()
    {
        var database = Guid.NewGuid().ToString();
        await using (var seed = Context(database))
        {
            seed.Opportunities.AddRange(OpportunityFor("me"), OpportunityFor("other"));
            await seed.SaveChangesAsync();
        }
        foreach (var user in new[] { "me", "other" })
        {
            var scope = new RecordPermissionScope();
            scope.RestrictToOwner("comercial", user, []);
            await using var db = Context(database, scope);
            Assert.Equal(user, (await db.Opportunities.SingleAsync()).ResponsibleId);
        }
    }

    private static WAssisDbContext Context(string database, RecordPermissionScope? scope = null) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseInMemoryDatabase(database).Options,
        new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "tenant", UserId = "me", BranchId = "branch", BranchIds = ["branch"] },
        recordScope: scope);

    private static Opportunity OpportunityFor(string responsible) => Opportunity.Create(
        "tenant", "branch", "Fixture", responsible, null, null, null, null, null, null,
        "pending", null, null, null, null, null, null, null, null, null, null, null);

    private static InsuredPerson Customer(string? producer, string? manager) => InsuredPerson.Create(
        tenantId: "tenant", officeBranchId: "branch", name: "Fixture", personType: "PF", status: "Prospecto",
        documentNumber: null, email: null, phoneNumber: null, birthDateUtc: null, tradeName: null, gender: null,
        maritalStatus: null, companySize: null, cnae: null, website: null, postalCode: null, street: null,
        number: null, complement: null, neighborhood: null, city: null, state: null, notes: null,
        producerId: producer, managerId: manager, chatwootId: null, lgpdAuthorized: false, createdBy: "me");
}
