using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.WhatsAppSupport.Interfaces;

public interface IWhatsAppConversationRepository
{
    Task AddAsync(WhatsAppConversation conversation, CancellationToken cancellationToken);
    Task<WhatsAppConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountWaitingHumanHandoffsAsync(CancellationToken cancellationToken);
    Task<int> CountHumanActiveAsync(CancellationToken cancellationToken);
    Task<PagedResult<WhatsAppConversationDto>> GetWaitingQueueAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
