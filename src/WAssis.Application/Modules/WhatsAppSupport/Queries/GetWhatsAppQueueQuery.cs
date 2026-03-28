using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed record GetWhatsAppQueueQuery() : IRequest<IReadOnlyCollection<WhatsAppConversationDto>>;
