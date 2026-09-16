using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed class GetWhatsAppQueueQueryHandler(IWhatsAppConversationRepository repository)
    : IRequestHandler<GetWhatsAppQueueQuery, PagedResult<WhatsAppConversationDto>>
{
    public Task<PagedResult<WhatsAppConversationDto>> Handle(GetWhatsAppQueueQuery request, CancellationToken cancellationToken)
    {
        return repository.GetWaitingQueueAsync(Math.Max(request.Page, 1), Math.Clamp(request.PageSize, 1, 100), cancellationToken);
    }
}
