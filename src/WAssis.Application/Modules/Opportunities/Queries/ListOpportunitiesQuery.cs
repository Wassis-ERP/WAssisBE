using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed record ListOpportunitiesQuery(string? PipelineId, string? StageId, string? Status, int Page, int PageSize) : IQuery<PagedResult<OpportunityDto>>;
