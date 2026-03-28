using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Billing.Commands;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Extensions;
using WAssis.Services.Api.Modules.Billing.Contracts;
using WAssis.Services.Api.Modules.Billing.ViewModels;

namespace WAssis.Services.Api.Modules.Billing.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize(Policy = AccessPolicies.BrokerageAdmin)]
public sealed class BillingSubscriptionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("subscriptions")]
    [ProducesResponseType(typeof(BillingSubscriptionViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateBillingSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateBillingSubscriptionCommand(
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.CustomerDisplayName,
                request.PlanCode,
                request.PlanName,
                request.Recurrence,
                request.Amount,
                request.BillingDayOfMonth,
                request.StartsAtUtc),
            cancellationToken);

        return this.ToActionResult(result, value =>
            CreatedAtAction(nameof(GetSubscriptionById), new { id = value.Id }, ToViewModel(value)));
    }

    [HttpGet("subscriptions/{id:guid}")]
    [ProducesResponseType(typeof(BillingSubscriptionViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBillingSubscriptionQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("subscriptions/{id:guid}/invoices")]
    [ProducesResponseType(typeof(BillingInvoiceViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> IssueInvoice(Guid id, [FromBody] IssueBillingInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new IssueBillingInvoiceCommand(
                id,
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.ReferencePeriod,
                request.DueDateUtc,
                request.ExternalReference,
                request.Notes),
            cancellationToken);

        return this.ToActionResult(result, value =>
            CreatedAtAction(nameof(GetInvoiceById), new { id = value.Id }, ToViewModel(value)));
    }

    [HttpGet("invoices/{id:guid}")]
    [ProducesResponseType(typeof(BillingInvoiceViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBillingInvoiceQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("invoices/{id:guid}/pay")]
    [ProducesResponseType(typeof(BillingInvoiceViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkBillingInvoicePaidRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new MarkBillingInvoicePaidCommand(id, request?.PaymentMethod, request?.ExternalReference, request?.Notes),
            cancellationToken);

        return this.ToActionResult(result, value => Ok(ToViewModel(value)));
    }

    private static BillingSubscriptionViewModel ToViewModel(BillingSubscriptionDto subscription)
    {
        return new BillingSubscriptionViewModel(
            subscription.Id,
            subscription.CorrelationId,
            subscription.CustomerDisplayName,
            subscription.PlanCode,
            subscription.PlanName,
            subscription.Recurrence,
            subscription.Amount,
            subscription.Currency,
            subscription.BillingDayOfMonth,
            subscription.Status,
            subscription.StartsAtUtc,
            subscription.NextInvoiceDueDateUtc,
            subscription.CreatedAtUtc,
            subscription.CanceledAtUtc,
            subscription.CancellationReason);
    }

    private static BillingInvoiceViewModel ToViewModel(BillingInvoiceDto invoice)
    {
        return new BillingInvoiceViewModel(
            invoice.Id,
            invoice.BillingSubscriptionId,
            invoice.CorrelationId,
            invoice.ReferencePeriod,
            invoice.Amount,
            invoice.Currency,
            invoice.DueDateUtc,
            invoice.Status,
            invoice.ExternalReference,
            invoice.PaymentMethod,
            invoice.Notes,
            invoice.IssuedAtUtc,
            invoice.PaidAtUtc);
    }
}
