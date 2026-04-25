using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Residence;

public sealed class LibertyResidenceQuoteProvider(IOptions<LibertyResidenceQuoteOptions> optionsAccessor)
    : LibertyBranchQuoteProviderBase<LibertyResidenceQuoteOptions>(optionsAccessor.Value)
{
    protected override string BranchName => "Residencial";
    protected override string ProviderCodeSuffix => "residence";
    protected override string BranchEvidenceMessageCode => "liberty_residence_endpoints_mapped";
    protected override string BranchEvidenceMessage => "Este provider pertence ao modulo residencial da seguradora e fica separado dos fluxos de vida, auto, empresarial e viagem.";
}
