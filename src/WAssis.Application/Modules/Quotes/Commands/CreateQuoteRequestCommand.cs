using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Commands;

public sealed record CreateQuoteRequestCommand(
    string CorrelationId,
    string CustomerName,
    string DocumentNumber,
    string? Email,
    string? PhoneNumber,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    int VehicleModelYear) : IRequest<QuoteRequestDto>;
