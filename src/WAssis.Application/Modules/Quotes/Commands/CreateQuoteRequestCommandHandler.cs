using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes.Commands;

public sealed class CreateQuoteRequestCommandHandler(
    IQuoteRequestRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<CreateQuoteRequestCommand, QuoteRequestDto>
{
    public async Task<QuoteRequestDto> Handle(CreateQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var existingQuoteRequest = await repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existingQuoteRequest is not null)
        {
            return QuoteRequestMappings.ToDto(existingQuoteRequest);
        }

        var quoteRequest = QuoteRequest.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
            request.CorrelationId,
            request.CustomerName,
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.PostalCode,
            request.CustomerSurname,
            request.CustomerGender,
            request.CustomerBirthDateUtc,
            request.VehiclePlate,
            request.VehicleBrand,
            request.VehicleModel,
            request.VehicleFipeCode,
            request.VehicleModelYear,
            request.HasDriverUnder24,
            request.IsCurrentlyInsured,
            request.PreviousBonus,
            request.BrokerCommissionPercentage,
            request.RenewalInsurerCode);

        await repository.AddAsync(quoteRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return QuoteRequestMappings.ToDto(quoteRequest);
    }
}
