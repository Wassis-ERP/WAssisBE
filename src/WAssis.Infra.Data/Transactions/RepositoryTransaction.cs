using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Transactions;

/// <summary>Atomic repository work, without committing or abandoning the request's RLS transaction.</summary>
internal sealed class RepositoryTransaction(IDbContextTransaction transaction, string? savepoint) : IAsyncDisposable
{
    private bool _completed;
    public DbTransaction DbTransaction => transaction.GetDbTransaction();

    public static async Task<RepositoryTransaction> BeginAsync(WAssisDbContext db, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is not { } current)
            return new(await db.Database.BeginTransactionAsync(cancellationToken), null);

        var name = "repository_" + Guid.NewGuid().ToString("N");
        await current.CreateSavepointAsync(name, cancellationToken);
        return new(current, name);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        if (savepoint is null) await transaction.CommitAsync(cancellationToken);
        else await transaction.ReleaseSavepointAsync(savepoint, cancellationToken);
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_completed)
            {
                if (savepoint is null) await transaction.RollbackAsync(CancellationToken.None);
                else
                {
                    await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None);
                    await transaction.ReleaseSavepointAsync(savepoint, CancellationToken.None);
                }
            }
        }
        finally
        {
            if (savepoint is null) await transaction.DisposeAsync();
        }
    }
}
