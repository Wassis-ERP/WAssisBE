using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed class AssignWhatsAppConversationCommandHandler(IWhatsAppConversationRepository repository)
    : IRequestHandler<AssignWhatsAppConversationCommand, WhatsAppConversationDto?>
{
    public async Task<WhatsAppConversationDto?> Handle(AssignWhatsAppConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return null;
        }

        conversation.Assign(request.AssignedToUserId, request.AssignedToDisplayName);
        await repository.SaveChangesAsync(cancellationToken);

        return WhatsAppConversationMappings.ToDto(conversation);
    }
}
