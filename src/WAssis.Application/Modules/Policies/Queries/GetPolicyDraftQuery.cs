using MediatR;
using WAssis.Application.Modules.Policies.Dtos;

namespace WAssis.Application.Modules.Policies.Queries;

public sealed record GetPolicyDraftQuery(Guid PolicyDraftId) : IRequest<PolicyDraftDto?>;
