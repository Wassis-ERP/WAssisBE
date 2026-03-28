using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes.Commands;

public sealed class CreateQuoteRequestCommandHandler(IQuoteRequestRepository repository)
    : IRequestHandler<CreateQuoteRequestCommand, QuoteRequestDto>
{
    public async Task<QuoteRequestDto> Handle(CreateQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var quoteRequest = QuoteRequest.Create(
            request.CorrelationId,
            request.CustomerName,
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.VehiclePlate,
            request.VehicleBrand,
            request.VehicleModel,
            request.VehicleModelYear);

        await repository.AddAsync(quoteRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return QuoteRequestMappings.ToDto(quoteRequest);
    }
}
