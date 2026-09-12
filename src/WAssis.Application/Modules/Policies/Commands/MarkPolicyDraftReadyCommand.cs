using WAssis.Application.Abstractions.Messaging;
using WAssis.Domain.Core.Messages;
using WAssis.Application.Modules.Policies.Dtos;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed record MarkPolicyDraftReadyCommand(Guid PolicyDraftId, string? Notes) : ITransactionalCommand<Result<PolicyDraftDto>>;
