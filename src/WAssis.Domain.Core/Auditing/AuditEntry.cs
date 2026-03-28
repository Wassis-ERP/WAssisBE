using WAssis.Domain.Core.Entities;

namespace WAssis.Domain.Core.Auditing;

public class AuditEntry : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuditEntry()
    {
    }

    private AuditEntry(
        Guid id,
        string tenantId,
        string correlationId,
        string module,
        string action,
        string entityType,
        string entityId,
        string? notes)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        Module = module;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Notes = notes;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static AuditEntry Create(
        string tenantId,
        string correlationId,
        string module,
        string action,
        string entityType,
        string entityId,
        string? notes)
    {
        return new AuditEntry(Guid.NewGuid(), tenantId, correlationId, module, action, entityType, entityId, notes);
    }
}
