using MediatR;
using WAssis.Application.Modules.Financial.Dtos;
using WAssis.Application.Modules.Financial.Interfaces;

namespace WAssis.Application.Modules.Financial.Queries;

public sealed class GetCommissionReconciliationQueryHandler(ICommissionReceiptRepository repository)
    : IRequestHandler<GetCommissionReconciliationQuery, CommissionReconciliationDto?>
{
    public async Task<CommissionReconciliationDto?> Handle(GetCommissionReconciliationQuery request, CancellationToken cancellationToken)
    {
        var reconciliation = await repository.GetReconciliationByIdAsync(request.ReconciliationId, cancellationToken);
        return reconciliation is null
            ? null
            : new CommissionReconciliationDto(
                reconciliation.Id,
                reconciliation.CommissionReceiptId,
                reconciliation.ExpectedAmount,
                reconciliation.ReceivedAmount,
                reconciliation.DifferenceAmount,
                reconciliation.Status,
                reconciliation.CreatedAtUtc);
    }
}
