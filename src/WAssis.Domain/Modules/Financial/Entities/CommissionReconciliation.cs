using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Financial.Enums;

namespace WAssis.Domain.Modules.Financial.Entities;

public class CommissionReconciliation : AggregateRoot
{
    public Guid CommissionReceiptId { get; private set; }
    public decimal ExpectedAmount { get; private set; }
    public decimal ReceivedAmount { get; private set; }
    public decimal DifferenceAmount { get; private set; }
    public CommissionReconciliationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private CommissionReconciliation()
    {
    }

    private CommissionReconciliation(Guid id, Guid commissionReceiptId, decimal expectedAmount, decimal receivedAmount)
    {
        Id = id;
        CommissionReceiptId = commissionReceiptId;
        ExpectedAmount = expectedAmount;
        ReceivedAmount = receivedAmount;
        DifferenceAmount = receivedAmount - expectedAmount;
        Status = DifferenceAmount == 0 ? CommissionReconciliationStatus.Matched : CommissionReconciliationStatus.Divergent;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommissionReconciliation Create(Guid commissionReceiptId, decimal expectedAmount, decimal receivedAmount)
    {
        return new CommissionReconciliation(Guid.NewGuid(), commissionReceiptId, expectedAmount, receivedAmount);
    }
}
