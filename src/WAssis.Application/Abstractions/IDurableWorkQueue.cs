namespace WAssis.Application.Abstractions;

public sealed record WorkLease(Guid Id, string TenantId, Guid AggregateId, Guid Token);

/// <summary>Technical outbox references typed aggregates; never stores business payloads.</summary>
public interface IDurableWorkQueue
{
    Task<WorkLease?> ClaimAsync(string kind, CancellationToken cancellationToken);
    Task RenewAsync(WorkLease lease, CancellationToken cancellationToken);
    Task CompleteAsync(WorkLease lease, CancellationToken cancellationToken);
    Task<int> QuarantineExpiredAsync(CancellationToken cancellationToken);
}
