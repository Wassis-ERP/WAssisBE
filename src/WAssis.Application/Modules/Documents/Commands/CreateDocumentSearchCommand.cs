using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed record CreateDocumentSearchCommand(
    string CorrelationId,
    string InsuranceCompanyCode,
    string SearchType) : IRequest<DocumentSearchDto>;
