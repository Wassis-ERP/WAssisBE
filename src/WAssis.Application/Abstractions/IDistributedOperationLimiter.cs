namespace WAssis.Application.Abstractions;

public sealed record OperationLimitDecision(bool Allowed, DateTimeOffset ResetAt);

public interface IDistributedOperationLimiter
{
    Task<OperationLimitDecision> AcquireAsync(
        string operation,
        string? tenantId,
        string? userId,
        string? remoteAddress,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken);
}
