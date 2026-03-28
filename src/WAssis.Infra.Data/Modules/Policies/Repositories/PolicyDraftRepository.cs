using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Policies.Repositories;

public sealed class PolicyDraftRepository(WAssisDbContext dbContext) : IPolicyDraftRepository
{
    public async Task AddAsync(PolicyDraft draft, CancellationToken cancellationToken)
    {
        await dbContext.PolicyDrafts.AddAsync(draft, cancellationToken);
    }

    public Task<PolicyDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.PolicyDrafts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
