using WAssis.Domain.Modules.Financial.Enums;

namespace WAssis.Application.Modules.Financial.Dtos;

public sealed record CommissionReconciliationDto(
    Guid Id,
    Guid CommissionReceiptId,
    decimal ExpectedAmount,
    decimal ReceivedAmount,
    decimal DifferenceAmount,
    CommissionReconciliationStatus Status,
    DateTime CreatedAtUtc);
