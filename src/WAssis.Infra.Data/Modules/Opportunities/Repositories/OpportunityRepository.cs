using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Infra.Data.Context;
using WAssis.Application.Abstractions;

namespace WAssis.Infra.Data.Modules.Opportunities.Repositories;

public sealed class OpportunityRepository(WAssisDbContext dbContext)
    : IOpportunityRepository, IOpportunityReadRepository
{
    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken)
    {
        await dbContext.Opportunities.AddAsync(opportunity, cancellationToken);
    }

    public Task<Opportunity?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Opportunities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Opportunities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Opportunity>> ListAsync(string? pipelineId, string? stageId, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Opportunities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pipelineId))
        {
            query = query.Where(x => x.Stage != null && x.Stage.PipelineId == pipelineId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stageId))
        {
            query = query.Where(x => x.StageId == stageId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status.Trim().ToLowerInvariant() switch
            {
                "won" => query.Where(x => x.WonAtUtc != null),
                "lost" => query.Where(x => x.WonAtUtc == null && x.LostAtUtc != null),
                "pending" => query.Where(x => x.WonAtUtc == null && x.LostAtUtc == null),
                _ => query.Where(x => false)
            };
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return new PagedResult<Opportunity>(items, page, pageSize, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
