using WAssis.Services.Api.Modules.Migration.Contracts;

namespace WAssis.Services.Api.Modules.Migration;

public static class FrontendContractCatalog
{
    public static readonly FrontendContractGapViewModel DataGatewayQuery = new(
        "data-gateway.query",
        "Supabase table query adapter",
        "Wassis-ERP/WAssisBE#6",
        "Create dedicated module endpoints for each table currently routed through the gateway.",
        null);

    public static readonly FrontendContractGapViewModel DataGatewayRpc = new(
        "data-gateway.rpc",
        "Supabase RPC adapter",
        "Wassis-ERP/WAssisBE#6",
        "Replace each RPC with a command/query endpoint owned by the matching backend module.",
        null);

    public static readonly FrontendContractGapViewModel LegacyStorage = new(
        "legacy.storage",
        "Supabase Storage adapter",
        "Wassis-ERP/WAssisBE#8",
        "Move file flows to Documents endpoints with backend authorization and tenant checks.",
        "/api/documents/*");

    public static readonly FrontendContractGapViewModel LegacyFunction = new(
        "legacy.function",
        "Supabase Edge Function adapter",
        "Wassis-ERP/WAssisBE#8",
        "Move legacy functions to explicit API endpoints or background workers.",
        null);

    public static readonly FrontendContractGapViewModel IdentityUsers = new(
        "identity.users.create",
        "Supabase Auth user creation",
        "Wassis-ERP/WAssisBE#8",
        "Implement internal user provisioning in the Identity module.",
        "/api/identity/users");

    public static IReadOnlyCollection<FrontendContractGapViewModel> All { get; } =
    [
        DataGatewayQuery,
        DataGatewayRpc,
        LegacyStorage,
        LegacyFunction,
        IdentityUsers
    ];
}
