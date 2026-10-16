using FluentValidation;
using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;

namespace WAssis.Application.Modules.Calculations;

public interface ICanonicalCalculationLookupStore
{
    Task<PagedResult<CalculationInsurerOption>?> ListInsurersAsync(Guid opportunityId, int page, int pageSize, string? search, CancellationToken token, bool includeReferencedInactive = false);
    Task<PagedResult<CalculationCoverageOption>?> ListCoveragesAsync(Guid opportunityId, Guid insuranceBranchId, int page, int pageSize, string? search, CancellationToken token);
    Task<CalculationEditorContext?> GetContextAsync(Guid opportunityId, Guid? insuranceBranchId, CancellationToken token);
}
public sealed record ListCalculationInsurersQuery(Guid OpportunityId, int Page = 1, int PageSize = 50, string? Search = null, bool IncludeReferencedInactive = false) : IQuery<PagedResult<CalculationInsurerOption>?>;
public sealed record ListCalculationCoveragesQuery(Guid OpportunityId, Guid InsuranceBranchId, int Page = 1, int PageSize = 50, string? Search = null) : IQuery<PagedResult<CalculationCoverageOption>?>;
public sealed record GetCalculationContextQuery(Guid OpportunityId, Guid? InsuranceBranchId = null) : IQuery<CalculationEditorContext?>;
public sealed class CalculationLookupHandlers(ICanonicalCalculationLookupStore store) :
    IRequestHandler<ListCalculationInsurersQuery, PagedResult<CalculationInsurerOption>?>,
    IRequestHandler<ListCalculationCoveragesQuery, PagedResult<CalculationCoverageOption>?>,
    IRequestHandler<GetCalculationContextQuery, CalculationEditorContext?>
{
    public Task<PagedResult<CalculationInsurerOption>?> Handle(ListCalculationInsurersQuery r, CancellationToken t) => store.ListInsurersAsync(r.OpportunityId, Math.Max(1, r.Page), Math.Clamp(r.PageSize, 1, 100), r.Search?.Trim(), t, r.IncludeReferencedInactive);
    public Task<PagedResult<CalculationCoverageOption>?> Handle(ListCalculationCoveragesQuery r, CancellationToken t) => store.ListCoveragesAsync(r.OpportunityId, r.InsuranceBranchId, Math.Max(1, r.Page), Math.Clamp(r.PageSize, 1, 100), r.Search?.Trim(), t);
    public Task<CalculationEditorContext?> Handle(GetCalculationContextQuery r, CancellationToken t) => store.GetContextAsync(r.OpportunityId, r.InsuranceBranchId, t);
}
public sealed class ListCalculationInsurersQueryValidator : AbstractValidator<ListCalculationInsurersQuery>
{
    public ListCalculationInsurersQueryValidator() { RuleFor(x => x.OpportunityId).NotEmpty(); RuleFor(x => x.Search).MaximumLength(100); }
}
public sealed class ListCalculationCoveragesQueryValidator : AbstractValidator<ListCalculationCoveragesQuery>
{
    public ListCalculationCoveragesQueryValidator() { RuleFor(x => x.OpportunityId).NotEmpty(); RuleFor(x => x.InsuranceBranchId).NotEmpty(); RuleFor(x => x.Search).MaximumLength(100); }
}
public sealed class GetCalculationContextQueryValidator : AbstractValidator<GetCalculationContextQuery>
{
    public GetCalculationContextQueryValidator() { RuleFor(x => x.OpportunityId).NotEmpty(); RuleFor(x => x.InsuranceBranchId).NotEqual(Guid.Empty); }
}
