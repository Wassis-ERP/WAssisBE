using Microsoft.EntityFrameworkCore;
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

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
