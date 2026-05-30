using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Clients;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Models;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto;

public sealed class JustosAutoQuoteProvider(
    IOptions<JustosAutoQuoteOptions> optionsAccessor,
    JustosBrokerAuthClient authClient,
    JustosQuoteClient quoteClient)
    : IQuoteProvider
{
    private readonly JustosAutoQuoteOptions _options = optionsAccessor.Value;

    public string ProviderCode => "justos_auto";
    public string ProviderName => "Justos Auto";
    public bool IsEnabled => _options.Enabled;

    public QuoteProviderDescriptorDto Describe()
    {
        var requirements = new[]
        {
            new QuoteProviderRequirementDto("broker_id", "Configurar o BrokerId da Justos.", !string.IsNullOrWhiteSpace(_options.BrokerId)),
            new QuoteProviderRequirementDto("issuer", "Configurar o issuer da credencial parceira.", !string.IsNullOrWhiteSpace(_options.Issuer)),
            new QuoteProviderRequirementDto("private_key_pem_path", "Disponibilizar a chave privada PEM usada para assinar o JWT ES256.", !string.IsNullOrWhiteSpace(_options.PrivateKeyPemPath))
        };

        return new QuoteProviderDescriptorDto(
            ProviderCode,
            ProviderName,
            IsEnabled,
            requirements.All(static requirement => requirement.IsConfigured),
            "JWT ES256 + API Token",
            "Auto",
            _options.DocumentationUrl,
            requirements,
            [
                new QuoteStatusMessageDto(
                    "justos_api_integrated",
                    "Provider de auto da Justos ja integrado ao fluxo canonico do multicalculo.")
            ]);
    }

    public async Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(
        StartQuoteProcessingDto request,
        CancellationToken cancellationToken)
    {
        var validationMessages = ValidateRequest(request);
        if (validationMessages.Count > 0)
        {
            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.Restriction,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    validationMessages)
            ];
        }

        var accessToken = await authClient.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var descriptor = Describe();

            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.LoginInvalid,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    descriptor.Messages
                        .Concat(
                            descriptor.Requirements
                                .Where(static requirement => !requirement.IsConfigured)
                                .Select(static requirement => new QuoteStatusMessageDto(
                                    $"{requirement.Code}_missing",
                                    requirement.Description)))
                        .Append(new QuoteStatusMessageDto(
                            "justos_auth_unavailable",
                            "Credenciais Justos Auto nao configuradas ou token nao foi obtido."))
                        .ToArray())
            ];
        }

        var payload = BuildQuoteRequest(request);
        using var response = await quoteClient.CreateQuoteAsync(accessToken, payload, cancellationToken);
        if (response is null)
        {
            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.Failure,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    [new QuoteStatusMessageDto("justos_quote_failed", "A API da Justos Auto nao retornou uma cotacao valida.")])
            ];
        }

        return ParseQuoteResponse(response, request);
    }

    private JustosQuoteRequest BuildQuoteRequest(StartQuoteProcessingDto request)
    {
        var commissionPercentage = request.BrokerCommissionPercentage
            ?? (int.TryParse(_options.DefaultCommissionPercentage, out var configuredPercentage)
                ? configuredPercentage
                : 15);

        return new JustosQuoteRequest(
            request.VehiclePlate!,
            request.PostalCode!,
            new JustosQuoteUser(
                request.DocumentNumber,
                request.CustomerName,
                ResolveSurname(request),
                request.CustomerGender!,
                request.CustomerBirthDateUtc!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            request.VehicleFipeCode!,
            request.VehicleModelYear.ToString(CultureInfo.InvariantCulture),
            request.HasDriverUnder24,
            request.IsCurrentlyInsured,
            string.IsNullOrWhiteSpace(request.PreviousBonus) ? "0" : request.PreviousBonus!,
            commissionPercentage,
            request.RenewalInsurerCode);
    }

    private IReadOnlyCollection<QuoteProviderResultDto> ParseQuoteResponse(JsonDocument response, StartQuoteProcessingDto request)
    {
        var root = response.RootElement;
        var quoteId = root.TryGetProperty("quote_id", out var quoteIdElement)
            ? quoteIdElement.ToString()
            : $"{ProviderCode}-{request.QuoteRequestId:N}";

        if (TryGetFirstArray(root, out var arrayElement))
        {
            var results = new List<QuoteProviderResultDto>();
            foreach (var item in arrayElement.EnumerateArray())
            {
                results.Add(new QuoteProviderResultDto(
                    ProviderCode,
                    ProviderName,
                    QuoteOptionStatus.Ok,
                    quoteId,
                    ExtractMoney(item, "premium", "premium_amount", "price", "total_price"),
                    null,
                    ExtractCoverages(item),
                    [],
                    [new QuoteStatusMessageDto("justos_quote_created", "Cotacao criada via API Justos Auto.")]));
            }

            if (results.Count > 0)
            {
                return results;
            }
        }

        return
        [
            new QuoteProviderResultDto(
                ProviderCode,
                ProviderName,
                QuoteOptionStatus.Ok,
                quoteId,
                ExtractMoney(root, "premium", "premium_amount", "price", "total_price"),
                null,
                [],
                [],
                [new QuoteStatusMessageDto("justos_quote_created", "Cotacao criada via API Justos Auto. Parsing detalhado de coberturas ainda em evolucao.")])
        ];
    }

    private static bool TryGetFirstArray(JsonElement root, out JsonElement arrayElement)
    {
        var matchingPropertyName = new[] { "coverages", "coverage_options", "options", "quotes" }
            .Where(propertyName => root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            .FirstOrDefault();

        if (matchingPropertyName is not null && root.TryGetProperty(matchingPropertyName, out arrayElement))
        {
            return true;
        }

        arrayElement = default;
        return false;
    }

    private static IReadOnlyCollection<CoverageSnapshotDto> ExtractCoverages(JsonElement item)
    {
        if (!TryGetFirstArray(item, out var coverageArray))
        {
            return [];
        }

        return coverageArray.EnumerateArray()
            .Select(static coverage => new CoverageSnapshotDto(
                ExtractString(coverage, "code", "coverage_code") ?? "coverage",
                ExtractString(coverage, "name", "title", "label") ?? "Coverage",
                ExtractMoney(coverage, "insured_amount", "insuredValue", "value"),
                ExtractMoney(coverage, "deductible_amount", "deductible")))
            .ToArray();
    }

    private static decimal? ExtractMoney(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
            {
                return number;
            }

            if (property.ValueKind == JsonValueKind.String)
            {
                var raw = property.GetString();
                if (!string.IsNullOrWhiteSpace(raw) &&
                    decimal.TryParse(raw.Replace(".", "").Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }
        }

        return null;
    }

    private static string? ExtractString(JsonElement element, params string[] propertyNames)
    {
        return propertyNames
            .Where(propertyName => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            .Select(propertyName => element.GetProperty(propertyName).GetString())
            .FirstOrDefault();
    }

    private static QuoteProviderResultDto CreateSingleResult(
        QuoteOptionStatus status,
        string externalReference,
        IReadOnlyCollection<QuoteStatusMessageDto> messages)
    {
        return new QuoteProviderResultDto(
            "justos_auto",
            "Justos Auto",
            status,
            externalReference,
            null,
            null,
            [],
            [],
            messages);
    }

    private static List<QuoteStatusMessageDto> ValidateRequest(StartQuoteProcessingDto request)
    {
        var messages = new List<QuoteStatusMessageDto>();

        if (string.IsNullOrWhiteSpace(request.VehiclePlate))
        {
            messages.Add(new QuoteStatusMessageDto("missing_plate", "A Justos Auto exige placa do veiculo."));
        }

        if (string.IsNullOrWhiteSpace(request.PostalCode))
        {
            messages.Add(new QuoteStatusMessageDto("missing_postal_code", "A Justos Auto exige CEP do risco."));
        }

        if (request.CustomerBirthDateUtc is null)
        {
            messages.Add(new QuoteStatusMessageDto("missing_birth_date", "A Justos Auto exige data de nascimento."));
        }

        if (string.IsNullOrWhiteSpace(request.CustomerGender))
        {
            messages.Add(new QuoteStatusMessageDto("missing_gender", "A Justos Auto exige genero do segurado."));
        }

        if (string.IsNullOrWhiteSpace(request.VehicleFipeCode))
        {
            messages.Add(new QuoteStatusMessageDto("missing_fipe_code", "A Justos Auto exige codigo FIPE do veiculo."));
        }

        return messages;
    }

    private static string ResolveSurname(StartQuoteProcessingDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.CustomerSurname))
        {
            return request.CustomerSurname.Trim();
        }

        var parts = request.CustomerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[^1] : "Cliente";
    }
}
