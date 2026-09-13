using Microsoft.EntityFrameworkCore;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Customers.Repositories;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Customers;

public sealed class InsuredPersonReadRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_DoesNotTrackReadModel()
    {
        var options = new DbContextOptionsBuilder<WAssisDbContext>()
            .UseInMemoryDatabase($"insured-read-{Guid.NewGuid():N}")
            .Options;
        await using var dbContext = new WAssisDbContext(options, new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "tenant-1", BranchId = "branch-1" });
        var insuredPerson = CreateInsuredPerson();
        dbContext.InsuredPeople.Add(insuredPerson);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var repository = new InsuredPersonRepository(dbContext);

        var result = await repository.GetByIdAsync(insuredPerson.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(dbContext.ChangeTracker.Entries<InsuredPerson>());
    }

    [Fact]
    public async Task GetForUpdateAsync_TracksAggregate()
    {
        var options = new DbContextOptionsBuilder<WAssisDbContext>()
            .UseInMemoryDatabase($"insured-write-{Guid.NewGuid():N}")
            .Options;
        await using var dbContext = new WAssisDbContext(options, new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "tenant-1", BranchId = "branch-1" });
        var insuredPerson = CreateInsuredPerson();
        dbContext.InsuredPeople.Add(insuredPerson);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var repository = new InsuredPersonRepository(dbContext);

        var result = await repository.GetForUpdateAsync(insuredPerson.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(dbContext.ChangeTracker.Entries<InsuredPerson>());
    }

    private static InsuredPerson CreateInsuredPerson()
    {
        return InsuredPerson.Create(
            tenantId: "tenant-1",
            officeBranchId: "branch-1",
            name: "Cliente",
            personType: "PF",
            status: "Ativo",
            documentNumber: null,
            email: null,
            phoneNumber: null,
            birthDateUtc: null,
            tradeName: null,
            gender: null,
            maritalStatus: null,
            companySize: null,
            cnae: null,
            website: null,
            postalCode: null,
            street: null,
            number: null,
            complement: null,
            neighborhood: null,
            city: null,
            state: null,
            notes: null,
            producerId: null,
            managerId: null,
            chatwootId: null,
            lgpdAuthorized: false,
            createdBy: "user-1");
    }
}
