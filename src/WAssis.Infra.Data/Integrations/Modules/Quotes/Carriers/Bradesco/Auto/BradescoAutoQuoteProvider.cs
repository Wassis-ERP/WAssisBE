using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Bradesco.Auto;

public sealed class BradescoAutoQuoteProvider(IOptions<BradescoAutoQuoteOptions> optionsAccessor) : IQuoteProvider
{
    private readonly BradescoAutoQuoteOptions _options = optionsAccessor.Value;

    public string ProviderCode => "bradesco_auto";
    public string ProviderName => "Bradesco Seguros Auto";
    public bool IsEnabled => _options.Enabled;

    public QuoteProviderDescriptorDto Describe()
    {
        var requirements = new[]
        {
            new QuoteProviderRequirementDto("product_line", "Selecionar o produto ou jornada de auto da Bradesco Seguros a integrar.", !string.IsNullOrWhiteSpace(_options.ProductLine)),
            new QuoteProviderRequirementDto("oauth_token_url", "Configurar a URL de autenticacao OAuth/JWT da Bradesco.", !string.IsNullOrWhiteSpace(_options.TokenUrl)),
            new QuoteProviderRequirementDto("client_id", "Configurar o ClientId da aplicacao parceira.", !string.IsNullOrWhiteSpace(_options.ClientId)),
            new QuoteProviderRequirementDto("client_secret", "Configurar o ClientSecret da aplicacao parceira.", !string.IsNullOrWhiteSpace(_options.ClientSecret)),
            new QuoteProviderRequirementDto(
                "mutual_tls_certificate",
                "Disponibilizar certificado cliente para mTLS quando exigido pela jornada de auto da Bradesco.",
                !_options.RequiresMutualTls ||
                !string.IsNullOrWhiteSpace(_options.ClientCertificatePath) ||
                !string.IsNullOrWhiteSpace(_options.ClientCertificateThumbprint))
        };

        var messages = new List<QuoteStatusMessageDto>
        {
            new(
                "bradesco_official_portal_catalogued",
                "Provider de auto modelado a partir do portal oficial da Bradesco Seguros. A chamada real depende da jornada contratada e das credenciais de parceiro.")
        };

        if (_options.RequiresMutualTls)
        {
            messages.Add(new QuoteStatusMessageDto(
                "bradesco_mtls_required",
                "A documentacao publica da Bradesco indica uso de certificado cliente (mTLS) em parte das APIs."));
        }

        return new QuoteProviderDescriptorDto(
            ProviderCode,
            ProviderName,
            IsEnabled,
            requirements.All(static requirement => requirement.IsConfigured),
            "OAuth 2.0 + JWT + mTLS",
            _options.ProductLine,
            _options.DocumentationUrl,
            requirements,
            messages);
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
                        "bradesco_product_mapping_pending",
                        "A integracao de auto da Bradesco segue aguardando o endpoint oficial de cotacao do produto selecionado."))
                    .ToArray())
        ];

        return Task.FromResult(results);
    }
}
