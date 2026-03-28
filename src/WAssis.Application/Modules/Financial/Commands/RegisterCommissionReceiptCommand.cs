using MediatR;
using WAssis.Application.Modules.Financial.Dtos;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed record RegisterCommissionReceiptCommand(
    string CorrelationId,
    string InsuranceCompanyCode,
    decimal ReceivedAmount,
    decimal ExpectedAmount,
    string SourceType,
    Guid? ImportedDocumentId) : IRequest<CommissionReconciliationDto>;
