using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Notifications.Dtos;
using WAssis.Application.Modules.Notifications.Interfaces;
using WAssis.Domain.Modules.Documents.Enums;
using WAssis.Domain.Modules.Financial.Enums;
using WAssis.Domain.Modules.Policies.Enums;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Notifications.Queries;

public sealed class OperationsDashboardReadRepository(WAssisDbContext dbContext) : IOperationsDashboardReadRepository
{
    public async Task<OperationsDashboardDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var last24h = DateTime.UtcNow.AddHours(-24);

        return new OperationsDashboardDto(
            await dbContext.QuoteRequests.CountAsync(x => x.Status == QuoteRequestStatus.Pending, cancellationToken),
            await dbContext.DocumentSearches.CountAsync(x => x.Status == DocumentSearchStatus.Pending, cancellationToken),
            await dbContext.ImportedDocuments.CountAsync(x => x.Status == ImportedDocumentStatus.Failed, cancellationToken),
            await dbContext.ImportedDocuments.CountAsync(x => x.RequiresHumanReview, cancellationToken),
            await dbContext.CommissionReconciliations.CountAsync(x => x.Status == CommissionReconciliationStatus.Divergent, cancellationToken),
            await dbContext.CommissionReconciliations.CountAsync(x => x.Status == CommissionReconciliationStatus.Settled && x.SettledAtUtc >= last24h, cancellationToken),
            await dbContext.PolicyDrafts.CountAsync(x => x.Status == PolicyDraftStatus.NeedsReview, cancellationToken),
            await dbContext.WhatsAppConversations.CountAsync(x => x.Status == WhatsAppConversationStatus.WaitingHumanHandoff, cancellationToken),
            await dbContext.WhatsAppConversations.CountAsync(x => x.Status == WhatsAppConversationStatus.HumanActive, cancellationToken),
            await dbContext.AuditEntries.CountAsync(x => x.CreatedAtUtc >= last24h, cancellationToken));
    }
}
