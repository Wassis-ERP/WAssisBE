namespace WAssis.Services.Api.Modules.Documents.Contracts;

public sealed record CreateDocumentSearchRequest(
    string? CorrelationId,
    string InsuranceCompanyCode,
    string SearchType);
