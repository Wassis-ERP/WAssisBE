using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.WhatsAppSupport.Repositories;

public sealed class WhatsAppConversationRepository(WAssisDbContext dbContext) : IWhatsAppConversationRepository
{
    public async Task AddAsync(WhatsAppConversation conversation, CancellationToken cancellationToken)
    {
        await dbContext.WhatsAppConversations.AddAsync(conversation, cancellationToken);
    }

    public Task<WhatsAppConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.WhatsAppConversations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<int> CountWaitingHumanHandoffsAsync(CancellationToken cancellationToken)
    {
        return dbContext.WhatsAppConversations.CountAsync(x => x.Status == WhatsAppConversationStatus.WaitingHumanHandoff, cancellationToken);
    }

    public Task<int> CountHumanActiveAsync(CancellationToken cancellationToken)
    {
        return dbContext.WhatsAppConversations.CountAsync(x => x.Status == WhatsAppConversationStatus.HumanActive, cancellationToken);
    }

    public async Task<IReadOnlyCollection<WhatsAppConversationDto>> GetWaitingQueueAsync(CancellationToken cancellationToken)
    {
        return await dbContext.WhatsAppConversations
            .AsNoTracking()
            .Where(x => x.Status == WhatsAppConversationStatus.WaitingHumanHandoff || x.Status == WhatsAppConversationStatus.HumanActive)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.SlaDueAtUtc)
            .Select(x => new WhatsAppConversationDto(
                x.Id,
                x.CorrelationId,
                x.CustomerIdentifier,
                x.LastMessagePreview,
                x.Status,
                x.Priority,
                x.AssignedToUserId,
                x.AssignedToDisplayName,
                x.SlaDueAtUtc,
                x.CreatedAtUtc,
                x.StartedHumanAtUtc,
                x.ClosedAtUtc,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
