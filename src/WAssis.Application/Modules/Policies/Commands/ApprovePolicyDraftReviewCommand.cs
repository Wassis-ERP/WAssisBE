using MediatR;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed record ApprovePolicyDraftReviewCommand(
    Guid PolicyDraftId,
    string? Notes) : IRequest<Result<PolicyDraftDto>>;
