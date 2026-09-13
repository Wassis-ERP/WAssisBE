using System.Text.Json;
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
    IAuditTrailWriter auditTrailWriter,
    IDurableWorkQueue workQueue)
    : IQuoteProcessingService
{
    public async Task<int> ProcessPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        await workQueue.QuarantineExpiredAsync(cancellationToken);
        // Also samples queue metrics; dispatch ownership is decided only by the atomic claim below.
        await repository.GetPendingDispatchBatchAsync(1, cancellationToken);
        var providers = await providerRegistry.GetEnabledProvidersAsync(cancellationToken);
        var processedCount = 0;

        for (var index = 0; index < Math.Clamp(batchSize, 1, 100); index++)
        {
            var lease = await workQueue.ClaimAsync("quotes.dispatch", cancellationToken);
            if (lease is null) break;
            var quoteRequest = await repository.GetByIdAsync(lease.AggregateId, cancellationToken);
            if (quoteRequest is null || quoteRequest.TenantId != lease.TenantId || quoteRequest.Status != QuoteRequestStatus.Pending)
            {
                // Preserve the lease for quarantine/reconciliation; never call an insurer for inconsistent work.
                continue;
            }

            quoteRequest.MarkAsProcessing();
            await repository.SaveChangesAsync(cancellationToken);

            using var activity = WorkerMetrics.Traces.StartActivity("quotes.dispatch", System.Diagnostics.ActivityKind.Consumer);
            activity?.SetTag("worker", "quotes");
            if (Guid.TryParse(quoteRequest.CorrelationId, out var correlation)) activity?.SetTag("wassis.correlation_id", correlation.ToString());

            var request = ToProcessingDto(quoteRequest);

            foreach (var provider in providers)
            {
                await workQueue.RenewAsync(lease, cancellationToken);
                IReadOnlyCollection<QuoteProviderResultDto> results;

                try
                {
                    results = await provider.StartQuoteAsync(request, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (HttpRequestException)
                {
                    results = CreateProviderFailureResult(provider, quoteRequest, "Falha de comunicação; confira o resultado na seguradora antes de reenviar.");
                }
                catch (JsonException)
                {
                    results = CreateProviderFailureResult(provider, quoteRequest, "Resposta inválida da seguradora.");
                }
                catch (InvalidOperationException)
                {
                    results = CreateProviderFailureResult(provider, quoteRequest, "Operação indisponível na seguradora.");
                }
                catch (TimeoutException)
                {
                    results = CreateProviderFailureResult(provider, quoteRequest, "Tempo limite excedido; confira o resultado antes de reenviar.");
                }
                catch (TaskCanceledException)
                {
                    results = CreateProviderFailureResult(provider, quoteRequest, "Operação interrompida; confira o resultado antes de reenviar.");
                }

                foreach (var result in results)
                {
                    if (result.Status is QuoteOptionStatus.Failure or QuoteOptionStatus.LoginInvalid)
                        WorkerMetrics.ProviderFailures.Add(1, new KeyValuePair<string, object?>("provider", provider.ProviderCode));
                    quoteRequest.AddOption(ToEntity(quoteRequest, result));
                }
            }

            quoteRequest.MarkAsCompleted();
            await workQueue.CompleteAsync(lease, cancellationToken);
            await auditTrailWriter.WriteAsync(
                quoteRequest.CorrelationId,
                "Quotes",
                "ProcessedPendingRequest",
                nameof(QuoteRequest),
                quoteRequest.Id.ToString(),
                $"Providers={providers.Count}; Status={quoteRequest.Status}",
                cancellationToken, quoteRequest.TenantId);

            processedCount++;
        }

        WorkerMetrics.Processed.Add(processedCount, new KeyValuePair<string, object?>("worker", "quotes"));
        WorkerMetrics.Duration.Record(System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalSeconds, new KeyValuePair<string, object?>("worker", "quotes"));
        return processedCount;
    }

    private static StartQuoteProcessingDto ToProcessingDto(QuoteRequest quoteRequest)
    {
        return new StartQuoteProcessingDto(
            quoteRequest.Id,
            quoteRequest.CorrelationId,
            quoteRequest.CustomerName,
            quoteRequest.DocumentNumber,
            quoteRequest.PhoneNumber,
            quoteRequest.PostalCode,
            quoteRequest.CustomerSurname,
            quoteRequest.CustomerGender,
            quoteRequest.CustomerMaritalStatusCode,
            quoteRequest.CustomerBirthDateUtc,
            quoteRequest.DriverLicenseYears,
            quoteRequest.DriverLicenseNumber,
            quoteRequest.InsuredDriverRelationshipCode,
            quoteRequest.VehicleChassisNumber,
            quoteRequest.VehiclePlate,
            quoteRequest.VehicleBrand,
            quoteRequest.VehicleModel,
            quoteRequest.VehicleFipeCode,
            quoteRequest.VehicleManufactureYear,
            quoteRequest.VehicleModelYear,
            quoteRequest.VehicleIsZeroKm,
            quoteRequest.VehicleHasTracker,
            quoteRequest.VehicleHasAntiTheft,
            quoteRequest.VehicleIsFinanced,
            quoteRequest.VehicleIsArmored,
            quoteRequest.VehicleFuelTypeCode,
            quoteRequest.VehicleOvernightPostalCode,
            quoteRequest.VehicleHasKitGas,
            quoteRequest.HasDriverUnder24,
            quoteRequest.IsCurrentlyInsured,
            quoteRequest.PreviousBonus,
            quoteRequest.BrokerCommissionPercentage,
            quoteRequest.RenewalInsurerCode);
    }

    private static QuoteProviderResultDto[] CreateProviderFailureResult(
        IQuoteProvider provider,
        QuoteRequest quoteRequest,
        string message)
    {
        return
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
                [new QuoteStatusMessageDto("provider_failure", message)])
        ];
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
