using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Customers.Repositories;

public sealed class InsuredPersonRepository(WAssisDbContext dbContext)
    : IInsuredPersonRepository, IInsuredPersonReadRepository
{
    public async Task AddAsync(InsuredPerson insuredPerson, CancellationToken cancellationToken)
    {
        await dbContext.InsuredPeople.AddAsync(insuredPerson, cancellationToken);
    }

    public Task<InsuredPerson?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.InsuredPeople.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<InsuredPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.InsuredPeople
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InsuredPerson>> ListAsync(string? search, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.InsuredPeople.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                (x.DocumentNumber != null && EF.Functions.ILike(x.DocumentNumber, pattern)) ||
                (x.Email != null && EF.Functions.ILike(x.Email, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query
            .OrderBy(x => x.Name)
            .Take(200)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
