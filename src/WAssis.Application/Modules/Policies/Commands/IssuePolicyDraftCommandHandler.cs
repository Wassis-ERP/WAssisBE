using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed class IssuePolicyDraftCommandHandler(
    IPolicyDraftRepository repository,
    IAuditTrailWriter auditTrailWriter)
    : IRequestHandler<IssuePolicyDraftCommand, Result<PolicyDraftDto>>
{
    public async Task<Result<PolicyDraftDto>> Handle(IssuePolicyDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = await repository.GetByIdAsync(request.PolicyDraftId, cancellationToken);
        if (draft is null)
        {
            return Result<PolicyDraftDto>.Failure(
                Error.NotFound("policy_draft_not_found", "Rascunho de proposta não encontrado."));
        }

        var policyNumber = string.IsNullOrWhiteSpace(request.PolicyNumber)
            ? $"WASSIS-{DateTime.UtcNow:yyyyMMdd}-{draft.Id.ToString("N")[..8].ToUpperInvariant()}"
            : request.PolicyNumber.Trim().ToUpperInvariant();

        draft.MarkIssued(policyNumber, request.Notes);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrailWriter.WriteAsync(
            draft.CorrelationId,
            "Policies",
            "Issue",
            nameof(Domain.Modules.Policies.Entities.PolicyDraft),
            draft.Id.ToString(),
            $"PolicyNumber={policyNumber}. {request.Notes}".Trim(),
            cancellationToken);

        return Result<PolicyDraftDto>.Success(PolicyDraftMappings.ToDto(draft));
    }
}
