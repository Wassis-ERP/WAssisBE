using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Security;

public sealed class PostgresOperationLimiter(WAssisDbContext db) : IDistributedOperationLimiter
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
        var key = Hash($"{operation}|{tenantId ?? "anonymous"}|{userId ?? "anonymous"}|{remoteAddress ?? "unknown"}");
        const string sql = """
            INSERT INTO security.rate_limit_buckets (bucket_key, window_started_at, request_count, expires_at)
            VALUES (@Key, now(), 1, now() + @Window)
            ON CONFLICT (bucket_key) DO UPDATE SET
              window_started_at = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                       THEN now() ELSE security.rate_limit_buckets.window_started_at END,
              request_count = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                   THEN 1 ELSE security.rate_limit_buckets.request_count + 1 END,
              expires_at = CASE WHEN security.rate_limit_buckets.expires_at <= now()
                                THEN now() + @Window ELSE security.rate_limit_buckets.expires_at END
            RETURNING request_count, expires_at
            """;
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<BucketResult>(new CommandDefinition(
            sql, new { Key = key, Window = window }, cancellationToken: cancellationToken));
        return new OperationLimitDecision(row.RequestCount <= permitLimit, row.ExpiresAt);
    }

    private static string Hash(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record BucketResult(int RequestCount, DateTimeOffset ExpiresAt);
}
