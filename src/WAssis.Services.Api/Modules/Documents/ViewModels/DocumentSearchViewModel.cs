using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Services.Api.Modules.Documents.ViewModels;

public sealed record DocumentSearchViewModel(
    Guid Id,
    string CorrelationId,
    string InsuranceCompanyCode,
    string SearchType,
    DocumentSearchStatus Status,
    DateTime CreatedAtUtc);
