using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record QuoteRequestDto(
    Guid Id,
    string CorrelationId,
    string CustomerName,
    string DocumentNumber,
    string? Email,
    string? PhoneNumber,
    string? PostalCode,
    string? CustomerSurname,
    string? CustomerGender,
    DateTime? CustomerBirthDateUtc,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    string? VehicleFipeCode,
    int VehicleModelYear,
    bool HasDriverUnder24,
    bool IsCurrentlyInsured,
    string? PreviousBonus,
    int? BrokerCommissionPercentage,
    int? RenewalInsurerCode,
    QuoteRequestStatus Status,
    string? ShareToken,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<QuoteOptionDto> Options);
