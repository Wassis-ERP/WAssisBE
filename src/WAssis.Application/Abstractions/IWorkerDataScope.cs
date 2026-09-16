namespace WAssis.Application.Abstractions;

/// <summary>
/// Runs one short database unit of work with transaction-local tenant and branch
/// context. External calls must never execute inside this scope.
/// </summary>
public interface IWorkerDataScope
{
    Task<T> ExecuteAsync<T>(
        string tenantId,
        string branchId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);
}
