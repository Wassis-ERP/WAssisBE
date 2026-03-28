namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record StartQuoteProcessingDto(
    Guid QuoteRequestId,
    string CorrelationId,
    string CustomerName,
    string DocumentNumber,
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
    int? RenewalInsurerCode);
