using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record QuoteRequestDto(
    Guid Id,
    string CorrelationId,
    string CustomerName,
    string DocumentNumber,
    string? Email,
    string? PhoneNumber,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    int VehicleModelYear,
    QuoteRequestStatus Status,
    string? ShareToken,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<QuoteOptionDto> Options);
