using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Opportunities.Repositories;

public sealed class OpportunityRepository(WAssisDbContext dbContext) : IOpportunityRepository
{
    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken)
    {
        await dbContext.Opportunities.AddAsync(opportunity, cancellationToken);
    }

    public Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Opportunities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Opportunity>> ListAsync(string? pipelineId, string? stageId, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.Opportunities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pipelineId))
        {
            query = query.Where(x => x.PipelineId == pipelineId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stageId))
        {
            query = query.Where(x => x.StageId == stageId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(300)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
