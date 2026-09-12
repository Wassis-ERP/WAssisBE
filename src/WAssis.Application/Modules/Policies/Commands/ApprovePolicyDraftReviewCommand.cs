using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed record ApprovePolicyDraftReviewCommand(
    Guid PolicyDraftId,
    string? Notes) : ITransactionalCommand<Result<PolicyDraftDto>>;
