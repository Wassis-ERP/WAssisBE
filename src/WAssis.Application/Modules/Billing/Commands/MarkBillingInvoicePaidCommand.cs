using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed record MarkBillingInvoicePaidCommand(
    Guid BillingInvoiceId,
    string? PaymentMethod,
    string? ExternalReference,
    string? Notes) : IRequest<Result<BillingInvoiceDto>>;
