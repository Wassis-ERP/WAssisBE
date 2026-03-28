using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.WhatsAppSupport.Commands;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.WhatsAppSupport.Contracts;
using WAssis.Services.Api.Modules.WhatsAppSupport.ViewModels;

namespace WAssis.Services.Api.Modules.WhatsAppSupport.Controllers;

[ApiController]
[Route("api/whatsapp/conversations")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
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
                request.RequestHumanHandoff,
                request.Priority),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToViewModel(result));
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWhatsAppQueueQuery(), cancellationToken);
        return Ok(result.Select(ToViewModel).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWhatsAppConversationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignWhatsAppConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignWhatsAppConversationCommand(id, request.AssignedToUserId, request.AssignedToDisplayName),
            cancellationToken);

        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseWhatsAppConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CloseWhatsAppConversationCommand(id, request.LastMessagePreview), cancellationToken);
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
            result.Priority,
            result.AssignedToUserId,
            result.AssignedToDisplayName,
            result.SlaDueAtUtc,
            result.CreatedAtUtc,
            result.StartedHumanAtUtc,
            result.ClosedAtUtc,
            result.UpdatedAtUtc);
    }
}
