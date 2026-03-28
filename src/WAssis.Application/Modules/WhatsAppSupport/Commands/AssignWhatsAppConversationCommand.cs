using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed record AssignWhatsAppConversationCommand(
    Guid ConversationId,
    string? AssignedToUserId,
    string? AssignedToDisplayName) : IRequest<WhatsAppConversationDto?>;
