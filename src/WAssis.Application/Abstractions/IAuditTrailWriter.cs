namespace WAssis.Application.Abstractions;

public interface IAuditTrailWriter
{
    Task WriteAsync(
        string correlationId,
        string module,
        string action,
        string entityType,
        string entityId,
        string? notes,
        CancellationToken cancellationToken);
}
