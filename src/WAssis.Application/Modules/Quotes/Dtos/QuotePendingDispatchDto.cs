namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record QuotePendingDispatchDto(
    Guid Id,
    string CorrelationId,
    DateTime CreatedAtUtc,
    string CustomerName,
    string? VehiclePlate);
