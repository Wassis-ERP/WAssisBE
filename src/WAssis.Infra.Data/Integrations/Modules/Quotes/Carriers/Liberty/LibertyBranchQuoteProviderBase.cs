using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

public abstract class LibertyBranchQuoteProviderBase<TOptions>(TOptions options) : IQuoteProvider
    where TOptions : LibertyQuoteOptionsBase
{
    protected TOptions Options { get; } = options;

    protected abstract string BranchName { get; }
    protected abstract string ProviderCodeSuffix { get; }
    protected abstract string BranchEvidenceMessageCode { get; }
    protected abstract string BranchEvidenceMessage { get; }

    public string ProviderCode => $"liberty_{ProviderCodeSuffix}";
    public string ProviderName => $"Liberty / Yelum {BranchName}";
    public bool IsEnabled => Options.Enabled;

    public QuoteProviderDescriptorDto Describe()
    {
        var requirements = new[]
        {
            new QuoteProviderRequirementDto("base_url", $"Configurar a BaseUrl do ambiente Liberty / Yelum habilitado para {BranchName.ToLowerInvariant()}.", !string.IsNullOrWhiteSpace(Options.BaseUrl)),
            new QuoteProviderRequirementDto("api_version", "Configurar a versao da API publicada pela Liberty / Yelum.", !string.IsNullOrWhiteSpace(Options.ApiVersion)),
            new QuoteProviderRequirementDto("product_line", $"Selecionar explicitamente a linha de {BranchName.ToLowerInvariant()} a integrar.", !string.IsNullOrWhiteSpace(Options.ProductLine)),
            new QuoteProviderRequirementDto("user", "Configurar o usuario tecnico exigido nas jornadas da Liberty / Yelum.", !string.IsNullOrWhiteSpace(Options.User)),
            new QuoteProviderRequirementDto("broker_code", "Configurar o codigo do corretor habilitado para a parceria.", !string.IsNullOrWhiteSpace(Options.BrokerCode)),
            new QuoteProviderRequirementDto("broker_branch_code", "Configurar o codigo da filial do corretor.", !string.IsNullOrWhiteSpace(Options.BrokerBranchCode)),
            new QuoteProviderRequirementDto("commercial_product_code", $"Configurar o CommercialProductCode da linha de {BranchName.ToLowerInvariant()}.", !string.IsNullOrWhiteSpace(Options.CommercialProductCode)),
            new QuoteProviderRequirementDto(
                "auth_contract",
                "Validar com a seguradora o contrato real de autenticacao antes de ativar chamadas HTTP.",
                !string.IsNullOrWhiteSpace(Options.AccessToken) ||
                (!string.IsNullOrWhiteSpace(Options.ClientId) && !string.IsNullOrWhiteSpace(Options.ClientSecret)))
        };

        return new QuoteProviderDescriptorDto(
            ProviderCode,
            ProviderName,
            IsEnabled,
            requirements.All(static requirement => requirement.IsConfigured),
            "A confirmar com a parceria Liberty / Yelum",
            Options.ProductLine,
            Options.DocumentationUrl,
            requirements,
            [
                new QuoteStatusMessageDto(
                    "liberty_openapi_catalogued",
                    "Provider modelado a partir do OpenAPI recebido da Liberty / Yelum."),
                new QuoteStatusMessageDto(BranchEvidenceMessageCode, BranchEvidenceMessage)
            ]);
    }

    public Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(
        StartQuoteProcessingDto request,
        CancellationToken cancellationToken)
    {
        var descriptor = Describe();

        IReadOnlyCollection<QuoteProviderResultDto> results =
        [
            new QuoteProviderResultDto(
                ProviderCode,
                ProviderName,
                descriptor.IsReady ? QuoteOptionStatus.Restriction : QuoteOptionStatus.LoginInvalid,
                $"{ProviderCode}-{request.QuoteRequestId:N}",
                null,
                null,
                [],
                [],
                descriptor.Messages
                    .Concat(
                        descriptor.Requirements
                            .Where(static requirement => !requirement.IsConfigured)
                            .Select(static requirement => new QuoteStatusMessageDto(
                                $"{requirement.Code}_missing",
                                requirement.Description)))
                    .Append(new QuoteStatusMessageDto(
                        "liberty_http_mapping_pending",
                        $"O modulo de {BranchName.ToLowerInvariant()} da Liberty foi registrado separadamente, mas a chamada real ainda depende da autenticacao validada com a seguradora e do fluxo dedicado deste ramo."))
                    .ToArray())
        ];

        return Task.FromResult(results);
    }
}
