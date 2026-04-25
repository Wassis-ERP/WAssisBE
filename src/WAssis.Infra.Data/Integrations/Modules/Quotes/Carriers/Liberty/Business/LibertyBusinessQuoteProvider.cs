using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Business;

public sealed class LibertyBusinessQuoteProvider(IOptions<LibertyBusinessQuoteOptions> optionsAccessor)
    : LibertyBranchQuoteProviderBase<LibertyBusinessQuoteOptions>(optionsAccessor.Value)
{
    protected override string BranchName => "Empresarial";
    protected override string ProviderCodeSuffix => "business";
    protected override string BranchEvidenceMessageCode => "liberty_business_endpoints_mapped";
    protected override string BranchEvidenceMessage => "Este provider pertence ao modulo empresarial da seguradora e fica separado dos fluxos de vida, auto, residencial e viagem.";
}
