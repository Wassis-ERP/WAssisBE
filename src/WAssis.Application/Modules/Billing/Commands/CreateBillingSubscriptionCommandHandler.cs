using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Domain.Modules.Billing.Entities;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed class CreateBillingSubscriptionCommandHandler(
    IBillingRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<CreateBillingSubscriptionCommand, Result<BillingSubscriptionDto>>
{
    public async Task<Result<BillingSubscriptionDto>> Handle(CreateBillingSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = BillingSubscription.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
            request.CorrelationId,
            request.CustomerDisplayName,
            request.PlanCode,
            request.PlanName,
            request.Recurrence,
            request.Amount,
            request.BillingDayOfMonth,
            request.StartsAtUtc);

        await repository.AddSubscriptionAsync(subscription, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<BillingSubscriptionDto>.Success(subscription.ToDto());
    }
}
