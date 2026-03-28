using MediatR;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed record RegisterWhatsAppInboundMessageCommand(
    string CorrelationId,
    string CustomerIdentifier,
    string MessagePreview,
    bool RequestHumanHandoff) : IRequest<WhatsAppConversationDto>;
