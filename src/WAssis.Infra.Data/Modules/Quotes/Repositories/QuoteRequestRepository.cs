using WAssis.Application.Modules.Quotes.Dtos;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Context;
using WAssis.Application.Abstractions;
using WAssis.Domain.Modules.Quotes;
using Npgsql;

namespace WAssis.Infra.Data.Modules.Quotes.Repositories;

public sealed class QuoteRequestRepository(WAssisDbContext dbContext) : IQuoteRequestRepository
{
    public async Task<QuoteRequest> CreateIdempotentAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        // Same key, across API instances: hold the lock until the request transaction commits.
        // A hash collision only serializes unrelated requests; equality still uses persisted fields.
        var lockKey = $"quotes.create:{quoteRequest.TenantId.Length}:{quoteRequest.TenantId}:{quoteRequest.CorrelationId}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);
        var existing = await GetByCorrelationIdAsync(quoteRequest.CorrelationId, cancellationToken);
        if (existing is not null)
        {
            if (!QuoteSubmissionComparer.Matches(existing, quoteRequest)) throw new QuoteRequestConflictException();
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        await AddAsync(quoteRequest, cancellationToken);
        try { await SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_quote_requests_TenantId_CorrelationId" })
        {
            // A tenant key outside the current branch is deliberately not read through RLS.
            throw new QuoteRequestConflictException();
        }
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        return quoteRequest;
    }

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

    public async Task<PagedResult<QuoteRequest>> ListAsync(
        Guid? opportunityId,
        string? officeBranchId,
        int page,
        int pageSize,
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

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return new PagedResult<QuoteRequest>(items, page, pageSize, totalCount);
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
                INSERT INTO infrastructure.work_outbox (id,tenant_id,branch_id,kind,aggregate_id)
                VALUES ({Guid.NewGuid()},{quote.TenantId},{quote.OfficeBranchId},'quotes.dispatch',{quote.Id})
                """, cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
    }
}
