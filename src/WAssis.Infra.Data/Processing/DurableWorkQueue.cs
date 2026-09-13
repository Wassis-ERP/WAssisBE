using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Processing;

public sealed class DurableWorkQueue(WAssisDbContext db, SystemDataScope? systemScope = null) : IDurableWorkQueue
{
    private void RequireWorker()
    {
        if (systemScope is null) throw new UnauthorizedAccessException("Explicit worker capability required.");
    }

    public async Task<WorkLease?> ClaimAsync(string kind, CancellationToken cancellationToken)
    {
        RequireWorker();
        return await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<WorkLease>(new CommandDefinition("""
            WITH candidate AS (
              SELECT id FROM infrastructure.work_outbox
              WHERE kind = @kind AND state = 'Pending'
              ORDER BY created_at, id FOR UPDATE SKIP LOCKED LIMIT 1
            )
            UPDATE infrastructure.work_outbox w
            SET state = 'Processing', lease_token = @token, lease_until = now() + interval '15 minutes', updated_at = now()
            FROM candidate c WHERE w.id = c.id
            RETURNING w.id AS "Id", w.tenant_id AS "TenantId", w.aggregate_id AS "AggregateId", w.lease_token AS "Token"
            """, new { kind, token = Guid.NewGuid() }, cancellationToken: cancellationToken));
    }

    public async Task RenewAsync(WorkLease lease, CancellationToken cancellationToken)
    {
        RequireWorker();
        var count = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE infrastructure.work_outbox SET lease_until = now() + interval '15 minutes', updated_at = now()
            WHERE id = {lease.Id} AND tenant_id = {lease.TenantId} AND lease_token = {lease.Token}
              AND state = 'Processing' AND lease_until > now()
            """, cancellationToken);
        if (count != 1) throw new InvalidOperationException("Work lease expired or ownership changed; reconcile before retrying.");
    }

    public async Task CompleteAsync(WorkLease lease, CancellationToken cancellationToken)
    {
        RequireWorker();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Fencing predicate and row lock precede aggregate writes. A stale worker cannot commit results.
        var count = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE infrastructure.work_outbox SET state = 'Completed', lease_until = null, updated_at = now()
            WHERE id = {lease.Id} AND tenant_id = {lease.TenantId} AND lease_token = {lease.Token}
              AND state = 'Processing' AND lease_until > now()
            """, cancellationToken);
        if (count != 1) throw new InvalidOperationException("Work lease cannot complete twice or after expiration.");
        await db.SaveChangesAsync(cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO infrastructure.work_inbox (work_id,tenant_id) VALUES ({lease.Id},{lease.TenantId})
            """, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<int> QuarantineExpiredAsync(CancellationToken cancellationToken)
    {
        RequireWorker();
        // No automatic re-dispatch: a remote insurer may already have accepted the request.
        return await db.Database.ExecuteSqlRawAsync("""
            UPDATE infrastructure.work_outbox SET state = 'NeedsReview', updated_at = now()
            WHERE state = 'Processing' AND lease_until <= now()
            """, cancellationToken);
    }
}
