using WAssis.Application.Modules.Quotes.Dtos;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Context;
using WAssis.Application.Abstractions;

namespace WAssis.Infra.Data.Modules.Quotes.Repositories;

public sealed class QuoteRequestRepository(WAssisDbContext dbContext) : IQuoteRequestRepository
{
    public async Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken)
    {
        await dbContext.QuoteRequests.AddAsync(quoteRequest, cancellationToken);
    }

    public Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.QuoteRequests
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
    {
        return dbContext.QuoteRequests
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<QuoteRequest>> ListAsync(
        Guid? opportunityId,
        string? officeBranchId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.QuoteRequests
            .AsNoTracking()
            .Include(x => x.Options)
            .AsQueryable();

        if (opportunityId.HasValue)
        {
            query = query.Where(x => x.OpportunityId == opportunityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(officeBranchId))
        {
            query = query.Where(x => x.OfficeBranchId == officeBranchId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pending = dbContext.QuoteRequests.Where(x => x.Status == QuoteRequestStatus.Pending);
        WorkerMetrics.RecordQueue(await pending.LongCountAsync(cancellationToken), await pending.MinAsync(x => (DateTime?)x.CreatedAtUtc, cancellationToken));
        return await dbContext.QuoteRequests
            .AsNoTracking()
            .Where(x => x.Status == QuoteRequestStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .Select(x => new QuotePendingDispatchDto(
                x.Id,
                x.CorrelationId,
                x.CreatedAtUtc,
                x.CustomerName,
                x.VehiclePlate))
            .ToArrayAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var added = dbContext.ChangeTracker.Entries<QuoteRequest>()
            .Where(entry => entry.State == EntityState.Added).Select(entry => entry.Entity).ToArray();
        if (added.Length == 0) { await dbContext.SaveChangesAsync(cancellationToken); return; }
        // Aggregate and dispatch reference commit together, including when a command owns the transaction.
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var quote in added)
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO infrastructure.work_outbox (id,tenant_id,kind,aggregate_id)
                VALUES ({Guid.NewGuid()},{quote.TenantId},'quotes.dispatch',{quote.Id})
                """, cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
    }
}
