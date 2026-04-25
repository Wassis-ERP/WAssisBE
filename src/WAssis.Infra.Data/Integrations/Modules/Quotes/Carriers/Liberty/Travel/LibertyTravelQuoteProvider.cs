using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Travel;

public sealed class LibertyTravelQuoteProvider(IOptions<LibertyTravelQuoteOptions> optionsAccessor)
    : LibertyBranchQuoteProviderBase<LibertyTravelQuoteOptions>(optionsAccessor.Value)
{
    protected override string BranchName => "Viagem";
    protected override string ProviderCodeSuffix => "travel";
    protected override string BranchEvidenceMessageCode => "liberty_travel_endpoints_mapped";
    protected override string BranchEvidenceMessage => "Este provider pertence ao modulo de viagem da seguradora e fica separado dos fluxos de vida, auto, residencial e empresarial.";
}
