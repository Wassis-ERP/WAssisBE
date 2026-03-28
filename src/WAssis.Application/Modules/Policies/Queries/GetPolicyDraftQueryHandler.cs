using MediatR;
using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Application.Modules.Policies.Interfaces;

namespace WAssis.Application.Modules.Policies.Queries;

public sealed class GetPolicyDraftQueryHandler(IPolicyDraftRepository repository)
    : IRequestHandler<GetPolicyDraftQuery, PolicyDraftDto?>
{
    public async Task<PolicyDraftDto?> Handle(GetPolicyDraftQuery request, CancellationToken cancellationToken)
    {
        var draft = await repository.GetByIdAsync(request.PolicyDraftId, cancellationToken);
        return draft is null ? null : PolicyDraftMappings.ToDto(draft);
    }
}
