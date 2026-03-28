using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed class GetWhatsAppConversationQueryHandler(IWhatsAppConversationRepository repository)
    : IRequestHandler<GetWhatsAppConversationQuery, WhatsAppConversationDto?>
{
    public async Task<WhatsAppConversationDto?> Handle(GetWhatsAppConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        return conversation is null
            ? null
            : new WhatsAppConversationDto(
                conversation.Id,
                conversation.CorrelationId,
                conversation.CustomerIdentifier,
                conversation.LastMessagePreview,
                conversation.Status,
                conversation.CreatedAtUtc,
                conversation.UpdatedAtUtc);
    }
}
