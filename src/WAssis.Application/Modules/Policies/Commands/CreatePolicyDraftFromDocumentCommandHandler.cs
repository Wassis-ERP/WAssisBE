using MediatR;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Domain.Modules.Documents.Enums;
using WAssis.Domain.Modules.Policies.Entities;

namespace WAssis.Application.Modules.Policies.Commands;

public sealed class CreatePolicyDraftFromDocumentCommandHandler(
    IImportedDocumentRepository importedDocumentRepository,
    IPolicyDraftRepository policyDraftRepository)
    : IRequestHandler<CreatePolicyDraftFromDocumentCommand, Result<PolicyDraftDto>>
{
    public async Task<Result<PolicyDraftDto>> Handle(CreatePolicyDraftFromDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await importedDocumentRepository.GetByIdAsync(request.ImportedDocumentId, cancellationToken);
        if (document is null)
        {
            return Result<PolicyDraftDto>.Failure(
                Error.NotFound("policy_document_not_found", "Documento importado não encontrado."));
        }

        if (document.Status != ImportedDocumentStatus.Parsed)
        {
            return Result<PolicyDraftDto>.Failure(
                Error.Conflict("policy_document_not_parsed", "Documento ainda não está parseado para criação de rascunho."));
        }

        var draft = PolicyDraft.Create(
            document.Id,
            document.CorrelationId,
            document.InsuranceCompanyName,
            document.ProposalNumber,
            document.InsuredName,
            document.CoverageStartDateUtc,
            document.CoverageEndDateUtc,
            document.TotalPremiumAmount,
            document.CommissionAmount,
            document.ParsingNotes);

        await policyDraftRepository.AddAsync(draft, cancellationToken);
        await policyDraftRepository.SaveChangesAsync(cancellationToken);

        return Result<PolicyDraftDto>.Success(PolicyDraftMappings.ToDto(draft));
    }
}
