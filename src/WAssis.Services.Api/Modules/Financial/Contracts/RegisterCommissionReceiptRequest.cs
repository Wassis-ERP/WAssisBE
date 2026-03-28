namespace WAssis.Services.Api.Modules.Financial.Contracts;

public sealed record RegisterCommissionReceiptRequest(
    string? CorrelationId,
    string InsuranceCompanyCode,
    decimal ReceivedAmount,
    decimal ExpectedAmount,
    string SourceType,
    Guid? ImportedDocumentId);
