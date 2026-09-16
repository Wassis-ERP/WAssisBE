using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;
using WAssis.Infra.Data.Context;
using WAssis.Application.Abstractions;

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

    public async Task<PagedResult<WhatsAppConversationDto>> GetWaitingQueueAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.WhatsAppConversations
            .AsNoTracking()
            .Where(x => x.Status == WhatsAppConversationStatus.WaitingHumanHandoff || x.Status == WhatsAppConversationStatus.HumanActive);
        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.SlaDueAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        return new PagedResult<WhatsAppConversationDto>(items, page, pageSize, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
