using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed class GetWhatsAppQueueQueryHandler(IWhatsAppConversationRepository repository)
    : IRequestHandler<GetWhatsAppQueueQuery, IReadOnlyCollection<WhatsAppConversationDto>>
{
    public Task<IReadOnlyCollection<WhatsAppConversationDto>> Handle(GetWhatsAppQueueQuery request, CancellationToken cancellationToken)
    {
        return repository.GetWaitingQueueAsync(cancellationToken);
    }
}
