using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;
using WAssis.Application.Abstractions;

namespace WAssis.Infra.Data.Security;

public sealed class PostgresOperationLimiter(NpgsqlDataSource dataSource) : IDistributedOperationLimiter
{
    public async Task<OperationLimitDecision> AcquireAsync(
        string operation,
        string? tenantId,
        string? userId,
        string? remoteAddress,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken)
    {
        if (window <= TimeSpan.Zero || window > TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(window));
        var partitions = OperationLimitPartitions.Create(operation, tenantId, userId, remoteAddress, permitLimit)
            .Select(p => new OperationLimitPartition(Hash(p.Key), p.PermitLimit))
            .OrderBy(p => p.Key, StringComparer.Ordinal).ToArray();
        const string sql = """
            INSERT INTO security.rate_limit_buckets (bucket_key, window_started_at, request_count, expires_at)
            VALUES (@Key, now(), 1, now() + @Window)
            ON CONFLICT (bucket_key) DO UPDATE SET
              window_started_at = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                       THEN now() ELSE security.rate_limit_buckets.window_started_at END,
              request_count = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                   THEN 1 ELSE LEAST(security.rate_limit_buckets.request_count::bigint + 1, 2147483647)::int END,
              expires_at = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                THEN now() + @Window ELSE security.rate_limit_buckets.expires_at END
            RETURNING request_count AS "RequestCount", expires_at AS "ExpiresAt"
            """;
        // A separate transaction commits consumption even if the business request
        // rolls back. Stable key order avoids deadlocks across replicas.
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var allowed = true;
        var resetAt = DateTimeOffset.MinValue;
        foreach (var partition in partitions)
        {
            var row = await connection.QuerySingleAsync<BucketResult>(new CommandDefinition(
                sql, new { Key = partition.Key, Window = window }, transaction, cancellationToken: cancellationToken));
            if (row.RequestCount > partition.PermitLimit)
            {
                allowed = false;
                if (row.ExpiresAt > resetAt) resetAt = row.ExpiresAt;
            }
            else if (allowed && row.ExpiresAt > resetAt) resetAt = row.ExpiresAt;
        }
        await transaction.CommitAsync(cancellationToken);
        return new OperationLimitDecision(allowed, resetAt);
    }

    private static string Hash(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record BucketResult(int RequestCount, DateTime ExpiresAt);
}
