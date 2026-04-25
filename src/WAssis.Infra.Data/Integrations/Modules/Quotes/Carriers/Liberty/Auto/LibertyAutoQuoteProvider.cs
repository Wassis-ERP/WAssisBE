using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Auto;

public sealed class LibertyAutoQuoteProvider(IOptions<LibertyAutoQuoteOptions> optionsAccessor)
    : LibertyBranchQuoteProviderBase<LibertyAutoQuoteOptions>(optionsAccessor.Value)
{
    protected override string BranchName => "Auto";
    protected override string ProviderCodeSuffix => "auto";
    protected override string BranchEvidenceMessageCode => "liberty_auto_endpoints_mapped";
    protected override string BranchEvidenceMessage => "Este provider pertence ao modulo de auto da seguradora e fica separado dos ramos de vida, residencial, empresarial e viagem. O YAML indica o endpoint generico /api/v{version}/Quote como candidato mais coerente para auto, enquanto /api/newsell/v{version}/Cotacao referencia schema de vida e ainda precisa de validacao com a parceria.";
}
