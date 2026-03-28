using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Domain.Modules.Quotes.ValueObjects;

namespace WAssis.Application.Modules.Quotes.Services;

public sealed class QuoteProcessingService(
    IQuoteRequestRepository repository,
    IQuoteProviderRegistry providerRegistry,
    IAuditTrailWriter auditTrailWriter)
    : IQuoteProcessingService
{
    public async Task<int> ProcessPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pendingQuotes = await repository.GetPendingDispatchBatchAsync(batchSize, cancellationToken);
        var providers = providerRegistry.GetEnabledProviders();
        var processedCount = 0;

        foreach (var pendingQuote in pendingQuotes)
        {
            var quoteRequest = await repository.GetByIdAsync(pendingQuote.Id, cancellationToken);
            if (quoteRequest is null || quoteRequest.Status != QuoteRequestStatus.Pending)
            {
                continue;
            }

            quoteRequest.MarkAsProcessing();
            await repository.SaveChangesAsync(cancellationToken);

            var request = ToProcessingDto(quoteRequest);

            foreach (var provider in providers)
            {
                IReadOnlyCollection<QuoteProviderResultDto> results;

                try
                {
                    results = await provider.StartQuoteAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    results =
                    [
                        new QuoteProviderResultDto(
                            provider.ProviderCode,
                            provider.ProviderName,
                            QuoteOptionStatus.Failure,
                            $"{provider.ProviderCode}-{quoteRequest.Id:N}",
                            null,
                            null,
                            [],
                            [],
                            [new QuoteStatusMessageDto("provider_failure", ex.Message)])
                    ];
                }

                foreach (var result in results)
                {
                    quoteRequest.AddOption(ToEntity(quoteRequest, result));
                }
            }

            quoteRequest.MarkAsCompleted();
            await repository.SaveChangesAsync(cancellationToken);
            await auditTrailWriter.WriteAsync(
                quoteRequest.CorrelationId,
                "Quotes",
                "ProcessedPendingRequest",
                nameof(QuoteRequest),
                quoteRequest.Id.ToString(),
                $"Providers={providers.Count}; Status={quoteRequest.Status}",
                cancellationToken);

            processedCount++;
        }

        return processedCount;
    }

    private static StartQuoteProcessingDto ToProcessingDto(QuoteRequest quoteRequest)
    {
        return new StartQuoteProcessingDto(
            quoteRequest.Id,
            quoteRequest.CorrelationId,
            quoteRequest.CustomerName,
            quoteRequest.DocumentNumber,
            quoteRequest.PostalCode,
            quoteRequest.CustomerSurname,
            quoteRequest.CustomerGender,
            quoteRequest.CustomerBirthDateUtc,
            quoteRequest.VehiclePlate,
            quoteRequest.VehicleBrand,
            quoteRequest.VehicleModel,
            quoteRequest.VehicleFipeCode,
            quoteRequest.VehicleModelYear,
            quoteRequest.HasDriverUnder24,
            quoteRequest.IsCurrentlyInsured,
            quoteRequest.PreviousBonus,
            quoteRequest.BrokerCommissionPercentage,
            quoteRequest.RenewalInsurerCode);
    }

    private static QuoteOption ToEntity(QuoteRequest quoteRequest, QuoteProviderResultDto result)
    {
        return new QuoteOption(
            Guid.NewGuid(),
            quoteRequest.TenantId,
            quoteRequest.Id,
            result.ProviderCode,
            result.ProviderName,
            result.ProviderCode,
            result.ProviderName,
            result.Status,
            result.PremiumAmount,
            result.CommissionAmount,
            result.ExternalReference,
            result.Coverages.Select(static x => new CoverageSnapshot(x.Code, x.Name, x.InsuredAmount, x.DeductibleAmount)),
            result.Installments.Select(static x => new InstallmentSnapshot(x.Number, x.Amount, x.TotalAmount)),
            result.Messages.Select(static x => new QuoteStatusMessage(x.Code, x.Description)));
    }
}
