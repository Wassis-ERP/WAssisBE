using WAssis.Application.Abstractions;
using WAssis.Domain.Core.Auditing;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Notifications.Repositories;

public sealed class AuditTrailWriter(WAssisDbContext dbContext) : IAuditTrailWriter
{
    public async Task WriteAsync(
        string correlationId,
        string module,
        string action,
        string entityType,
        string entityId,
        string? notes,
        CancellationToken cancellationToken)
    {
        var entry = AuditEntry.Create(correlationId, module, action, entityType, entityId, notes);
        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
