using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed record IssueBillingInvoiceCommand(
    Guid BillingSubscriptionId,
    string CorrelationId,
    string ReferencePeriod,
    DateTime? DueDateUtc,
    string? ExternalReference,
    string? Notes) : IRequest<Result<BillingInvoiceDto>>;
