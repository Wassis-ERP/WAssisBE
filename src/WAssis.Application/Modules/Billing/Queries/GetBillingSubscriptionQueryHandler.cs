using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Interfaces;

namespace WAssis.Application.Modules.Billing.Queries;

public sealed class GetBillingSubscriptionQueryHandler(IBillingRepository repository)
    : IRequestHandler<GetBillingSubscriptionQuery, BillingSubscriptionDto?>
{
    public async Task<BillingSubscriptionDto?> Handle(GetBillingSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await repository.GetSubscriptionByIdAsync(request.Id, cancellationToken);
        return subscription?.ToDto();
    }
}
