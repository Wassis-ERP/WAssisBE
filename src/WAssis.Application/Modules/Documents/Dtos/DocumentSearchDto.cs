using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Application.Modules.Documents.Dtos;

public sealed record DocumentSearchDto(
    Guid Id,
    string CorrelationId,
    string InsuranceCompanyCode,
    string SearchType,
    DocumentSearchStatus Status,
    DateTime CreatedAtUtc);
