using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed record CloseWhatsAppConversationCommand(
    Guid ConversationId,
    string LastMessagePreview) : IRequest<WhatsAppConversationDto?>;
