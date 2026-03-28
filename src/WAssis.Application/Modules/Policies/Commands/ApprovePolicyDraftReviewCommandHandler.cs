using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed class ApprovePolicyDraftReviewCommandHandler(
    IPolicyDraftRepository repository,
    ICurrentUserContext currentUserContext,
    IAuditTrailWriter auditTrailWriter)
    : IRequestHandler<ApprovePolicyDraftReviewCommand, Result<PolicyDraftDto>>
{
    public async Task<Result<PolicyDraftDto>> Handle(ApprovePolicyDraftReviewCommand request, CancellationToken cancellationToken)
    {
        var draft = await repository.GetByIdAsync(request.PolicyDraftId, cancellationToken);
        if (draft is null)
        {
            return Result<PolicyDraftDto>.Failure(
                Error.NotFound("policy_draft_not_found", "Rascunho de proposta nao encontrado."));
        }

        draft.ApproveReview(currentUserContext.UserId ?? "system-review", request.Notes);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrailWriter.WriteAsync(
            draft.CorrelationId,
            "Policies",
            "ApproveReview",
            nameof(Domain.Modules.Policies.Entities.PolicyDraft),
            draft.Id.ToString(),
            request.Notes,
            cancellationToken);

        return Result<PolicyDraftDto>.Success(PolicyDraftMappings.ToDto(draft));
    }
}
