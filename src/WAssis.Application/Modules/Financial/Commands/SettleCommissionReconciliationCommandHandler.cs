using MediatR;
using WAssis.Application.Modules.Financial.Dtos;
using WAssis.Application.Modules.Financial.Interfaces;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed class SettleCommissionReconciliationCommandHandler(ICommissionReceiptRepository repository)
    : IRequestHandler<SettleCommissionReconciliationCommand, CommissionReconciliationDto?>
{
    public async Task<CommissionReconciliationDto?> Handle(SettleCommissionReconciliationCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = await repository.GetReconciliationByIdAsync(request.ReconciliationId, cancellationToken);
        if (reconciliation is null)
        {
            return null;
        }

        reconciliation.MarkSettled(request.MatchedReference, request.SettlementNotes);
        await repository.SaveChangesAsync(cancellationToken);

        return new CommissionReconciliationDto(
            reconciliation.Id,
            reconciliation.CommissionReceiptId,
            reconciliation.ExpectedAmount,
            reconciliation.ReceivedAmount,
            reconciliation.DifferenceAmount,
            reconciliation.Status,
            reconciliation.MatchedReference,
            reconciliation.SettlementNotes,
            reconciliation.CreatedAtUtc,
            reconciliation.SettledAtUtc);
    }
}
