using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Application.Modules.WhatsAppSupport.Commands;

public sealed class RegisterWhatsAppInboundMessageCommandHandler(
    IWhatsAppConversationRepository repository,
    IAuditTrailWriter auditTrailWriter)
    : IRequestHandler<RegisterWhatsAppInboundMessageCommand, WhatsAppConversationDto>
{
    public async Task<WhatsAppConversationDto> Handle(RegisterWhatsAppInboundMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = WhatsAppConversation.Create(
            request.CorrelationId,
            request.CustomerIdentifier,
            request.MessagePreview);

        if (request.RequestHumanHandoff)
        {
            conversation.RequestHumanHandoff();
        }

        await repository.AddAsync(conversation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrailWriter.WriteAsync(
            request.CorrelationId,
            "WhatsAppSupport",
            request.RequestHumanHandoff ? "InboundWithHandoff" : "InboundMessage",
            nameof(WhatsAppConversation),
            conversation.Id.ToString(),
            request.MessagePreview,
            cancellationToken);

        return new WhatsAppConversationDto(
            conversation.Id,
            conversation.CorrelationId,
            conversation.CustomerIdentifier,
            conversation.LastMessagePreview,
            conversation.Status,
            conversation.CreatedAtUtc,
            conversation.UpdatedAtUtc);
    }
}
