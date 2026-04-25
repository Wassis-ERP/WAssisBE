namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Models;

public sealed record JustosApiTokenRequest(
    string Token,
    int BrokerId);
