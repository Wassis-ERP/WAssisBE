using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Life;

public sealed class LibertyLifeQuoteProvider(IOptions<LibertyLifeQuoteOptions> optionsAccessor)
    : LibertyBranchQuoteProviderBase<LibertyLifeQuoteOptions>(optionsAccessor.Value)
{
    protected override string BranchName => "Vida";
    protected override string ProviderCodeSuffix => "vida";
    protected override string BranchEvidenceMessageCode => "liberty_life_endpoints_mapped";
    protected override string BranchEvidenceMessage => "Este provider pertence ao modulo de vida da seguradora e fica separado das futuras integracoes de auto, residencial, empresarial e viagem.";
}
