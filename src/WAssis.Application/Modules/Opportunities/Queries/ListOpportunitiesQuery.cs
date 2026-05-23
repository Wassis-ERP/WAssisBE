using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed record ListOpportunitiesQuery(string? PipelineId, string? StageId, string? Status) : IRequest<IReadOnlyCollection<OpportunityDto>>;
