using MediatR;
using WAssis.Domain.Core.Messages;
using WAssis.Application.Modules.Policies.Dtos;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed record MarkPolicyDraftReadyCommand(Guid PolicyDraftId, string? Notes) : IRequest<Result<PolicyDraftDto>>;
