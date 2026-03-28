using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed class CloseWhatsAppConversationCommandHandler(IWhatsAppConversationRepository repository)
    : IRequestHandler<CloseWhatsAppConversationCommand, WhatsAppConversationDto?>
{
    public async Task<WhatsAppConversationDto?> Handle(CloseWhatsAppConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return null;
        }

        conversation.Close(request.LastMessagePreview);
        await repository.SaveChangesAsync(cancellationToken);

        return WhatsAppConversationMappings.ToDto(conversation);
    }
}
