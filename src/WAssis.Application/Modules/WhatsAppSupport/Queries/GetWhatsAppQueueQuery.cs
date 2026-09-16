using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.WhatsAppSupport.Queries;

public sealed record GetWhatsAppQueueQuery(int Page, int PageSize) : IRequest<PagedResult<WhatsAppConversationDto>>;
