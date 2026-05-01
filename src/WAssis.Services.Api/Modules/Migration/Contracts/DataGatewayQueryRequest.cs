namespace WAssis.Services.Api.Modules.Migration.Contracts;

public sealed record DataGatewayQueryRequest(
    string Table,
    string Operation,
    string? Selection,
    IReadOnlyCollection<DataGatewayFilterRequest>? Filters,
    IReadOnlyDictionary<string, object?>? Modifiers,
    object? Payload,
    bool Single);

public sealed record DataGatewayFilterRequest(string Operator, string Column, object? Value);
