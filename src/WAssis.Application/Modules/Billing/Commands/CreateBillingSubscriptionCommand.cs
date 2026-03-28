using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Domain.Core.Messages;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed record CreateBillingSubscriptionCommand(
    string CorrelationId,
    string CustomerDisplayName,
    string PlanCode,
    string PlanName,
    BillingRecurrence Recurrence,
    decimal Amount,
    int BillingDayOfMonth,
    DateTime StartsAtUtc) : IRequest<Result<BillingSubscriptionDto>>;
