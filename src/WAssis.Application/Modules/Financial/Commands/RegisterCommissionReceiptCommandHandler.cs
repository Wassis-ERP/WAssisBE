using MediatR;
using WAssis.Application.Modules.Financial.Dtos;
using WAssis.Application.Modules.Financial.Interfaces;
using WAssis.Domain.Modules.Financial.Entities;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed class RegisterCommissionReceiptCommandHandler(ICommissionReceiptRepository repository)
    : IRequestHandler<RegisterCommissionReceiptCommand, CommissionReconciliationDto>
{
    public async Task<CommissionReconciliationDto> Handle(RegisterCommissionReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = CommissionReceipt.Create(
            request.CorrelationId,
            request.InsuranceCompanyCode,
            request.ReceivedAmount,
            request.SourceType,
            request.ImportedDocumentId);

        var reconciliation = CommissionReconciliation.Create(
            receipt.Id,
            request.ExpectedAmount,
            request.ReceivedAmount);

        await repository.AddReceiptAsync(receipt, cancellationToken);
        await repository.AddReconciliationAsync(reconciliation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new CommissionReconciliationDto(
            reconciliation.Id,
            reconciliation.CommissionReceiptId,
            reconciliation.ExpectedAmount,
            reconciliation.ReceivedAmount,
            reconciliation.DifferenceAmount,
            reconciliation.Status,
            reconciliation.CreatedAtUtc);
    }
}
