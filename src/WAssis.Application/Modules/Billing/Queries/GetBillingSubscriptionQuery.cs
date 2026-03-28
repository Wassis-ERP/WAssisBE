using MediatR;
using WAssis.Application.Modules.Billing.Dtos;

namespace WAssis.Application.Modules.Billing.Queries;

public sealed record GetBillingSubscriptionQuery(Guid Id) : IRequest<BillingSubscriptionDto?>;
