using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed record RegisterWhatsAppInboundMessageCommand(
    string CorrelationId,
    string CustomerIdentifier,
    string MessagePreview,
    bool RequestHumanHandoff,
    WhatsAppConversationPriority Priority) : ITransactionalCommand<WhatsAppConversationDto>;
