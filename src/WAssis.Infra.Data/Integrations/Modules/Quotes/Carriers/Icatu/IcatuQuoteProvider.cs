using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Icatu;

public sealed class IcatuQuoteProvider(IOptions<IcatuQuoteOptions> optionsAccessor) : IQuoteProvider
{
    private readonly IcatuQuoteOptions _options = optionsAccessor.Value;

    public string ProviderCode => "icatu_seguros";
    public string ProviderName => "Icatu Seguros";
    public bool IsEnabled => _options.Enabled;

    public QuoteProviderDescriptorDto Describe()
    {
        var requirements = new[]
        {
            new QuoteProviderRequirementDto("product_line", "Selecionar o catalogo ou produto da Icatu Seguros a integrar.", !string.IsNullOrWhiteSpace(_options.ProductLine)),
            new QuoteProviderRequirementDto("api_catalog_key", "Configurar a chave do catalogo ou assinatura da API da Icatu.", !string.IsNullOrWhiteSpace(_options.ApiCatalogKey)),
            new QuoteProviderRequirementDto("client_id", "Configurar o ClientId ou identificador da aplicacao parceira.", !string.IsNullOrWhiteSpace(_options.ClientId)),
            new QuoteProviderRequirementDto("client_secret", "Configurar o segredo da aplicacao parceira.", !string.IsNullOrWhiteSpace(_options.ClientSecret)),
            new QuoteProviderRequirementDto("partner_id", "Informar o identificador do parceiro quando exigido pela Icatu.", !string.IsNullOrWhiteSpace(_options.PartnerId)),
            new QuoteProviderRequirementDto("application_id", "Informar o identificador da aplicacao consumidora.", !string.IsNullOrWhiteSpace(_options.ApplicationId)),
            new QuoteProviderRequirementDto("certificate_id", "Informar o identificador do certificado habilitado para a parceria.", !string.IsNullOrWhiteSpace(_options.CertificateId))
        };

        return new QuoteProviderDescriptorDto(
            ProviderCode,
            ProviderName,
            IsEnabled,
            requirements.All(static requirement => requirement.IsConfigured),
            "Credenciais de parceria por catalogo",
            _options.ProductLine,
            _options.DocumentationUrl,
            requirements,
            [
                new QuoteStatusMessageDto(
                    "icatu_official_portal_catalogued",
                    "Provider modelado a partir do portal oficial da Icatu. A chamada real depende do catalogo habilitado e das credenciais de parceria.")
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
                        "icatu_product_mapping_pending",
                        "A integracao da Icatu segue aguardando o detalhamento do endpoint oficial do produto selecionado."))
                    .ToArray())
        ];

        return Task.FromResult(results);
    }
}
