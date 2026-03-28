using WAssis.Domain.Modules.Financial.Enums;

namespace WAssis.Services.Api.Modules.Financial.ViewModels;

public sealed record CommissionReconciliationViewModel(
    Guid Id,
    Guid CommissionReceiptId,
    decimal ExpectedAmount,
    decimal ReceivedAmount,
    decimal DifferenceAmount,
    CommissionReconciliationStatus Status,
    DateTime CreatedAtUtc);
