using MediatR;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.WhatsAppSupport.Commands;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Queries;
using WAssis.Services.Api.Modules.WhatsAppSupport.Contracts;
using WAssis.Services.Api.Modules.WhatsAppSupport.ViewModels;

namespace WAssis.Services.Api.Modules.WhatsAppSupport.Controllers;

[ApiController]
[Route("api/whatsapp/conversations")]
public sealed class WhatsAppConversationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("inbound")]
    public async Task<IActionResult> RegisterInbound([FromBody] RegisterWhatsAppInboundMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterWhatsAppInboundMessageCommand(
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.CustomerIdentifier,
                request.MessagePreview,
                request.RequestHumanHandoff),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToViewModel(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWhatsAppConversationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    private static WhatsAppConversationViewModel ToViewModel(WhatsAppConversationDto result)
    {
        return new WhatsAppConversationViewModel(
            result.Id,
            result.CorrelationId,
            result.CustomerIdentifier,
            result.LastMessagePreview,
            result.Status,
            result.CreatedAtUtc,
            result.UpdatedAtUtc);
    }
}
