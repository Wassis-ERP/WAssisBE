using Microsoft.EntityFrameworkCore;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Configuration;

public static class DatabaseMigrationGate
{
    // Session advisory lock: held across EF migration transactions, released on process/connection exit.
    private const long LockId = 872391004201;

    public static async Task MigrateSingletonAsync(WAssisDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT pg_try_advisory_lock({LockId})";
            if (await command.ExecuteScalarAsync(cancellationToken) is not true)
                throw new InvalidOperationException("Another migration runner holds the database lock. Rollout must stop.");
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                await EnsureCurrentAsync(db, cancellationToken);
            }
            finally
            {
                command.CommandText = $"SELECT pg_advisory_unlock({LockId})";
                await command.ExecuteScalarAsync(CancellationToken.None);
            }
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }

    public static async Task EnsureCurrentAsync(WAssisDbContext db, CancellationToken cancellationToken = default)
    {
        if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            throw new InvalidOperationException("Database schema is behind this image. Run the singleton migration before rollout.");
    }
}
