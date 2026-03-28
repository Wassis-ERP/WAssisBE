using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed record GetWhatsAppConversationQuery(Guid ConversationId) : IRequest<WhatsAppConversationDto?>;
