using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Security;

public sealed class WorkerDataScope(WAssisDbContext db, SystemDataScope? systemScope = null) : IWorkerDataScope
{
    public async Task<T> ExecuteAsync<T>(
        string tenantId,
        string branchId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        if (systemScope is null)
            throw new UnauthorizedAccessException("Explicit worker capability required.");
        if (!Guid.TryParse(tenantId, out var parsedTenant)
            || !Guid.TryParse(branchId, out var parsedBranch))
            throw new InvalidOperationException("Worker item is missing a valid tenant or branch context.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            SELECT set_config('app.tenant_id', {parsedTenant.ToString()}, true),
                   set_config('app.branch_id', {parsedBranch.ToString()}, true),
                   set_config('app.user_id', '', true)
            """, cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            db.ChangeTracker.Clear();
        }
    }
}
