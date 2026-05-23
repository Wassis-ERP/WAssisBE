using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed record GetOpportunityQuery(Guid Id) : IRequest<OpportunityDto?>;
