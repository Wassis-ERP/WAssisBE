namespace WAssis.Services.Api.Modules.Quotes.Contracts;

public sealed record CreateQuoteRequestRequest(
    string? CorrelationId,
    string CustomerName,
    string DocumentNumber,
    string? Email,
    string? PhoneNumber,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    int VehicleModelYear);
