using WAssis.Application.Abstractions;
using WAssis.Domain.Core.Auditing;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Notifications.Repositories;

public sealed class AuditTrailWriter(
    WAssisDbContext dbContext,
    ICurrentUserContext currentUserContext,
    SystemDataScope? systemScope = null) : IAuditTrailWriter
{
    public async Task WriteAsync(
        string correlationId,
        string module,
        string action,
        string entityType,
        string entityId,
        string? notes,
        CancellationToken cancellationToken,
        string? tenantId = null)
    {
        var scope = systemScope is not null && !currentUserContext.IsAuthenticated
            ? tenantId : currentUserContext.ResolveTenantIdOrPlatform();
        if (string.IsNullOrWhiteSpace(scope) || (tenantId is not null && tenantId != scope))
            throw new UnauthorizedAccessException("Tenant explícito obrigatório para auditoria do worker.");
        var entry = AuditEntry.Create(
            scope,
            correlationId,
            module,
            action,
            entityType,
            entityId,
            notes);
        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
