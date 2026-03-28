namespace WAssis.Services.Api.Modules.Quotes.Contracts;

public sealed record CreateQuoteRequestRequest(
    string? CorrelationId,
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
    int? RenewalInsurerCode);
