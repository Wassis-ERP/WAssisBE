using FluentValidation;
using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;

namespace WAssis.Application.Modules.Calculations;

public interface ICanonicalPresentationStore
{
    Task<CommercialPresentationAggregate?> SaveAsync(Guid resourceId, bool update, string key, SaveCommercialPresentation input, CancellationToken token);
    Task<CommercialPresentationAggregate?> GetAsync(Guid id, CancellationToken token);
    Task<PagedResult<CommercialPresentationAggregate>?> ListAsync(Guid opportunityId, int page, int pageSize, CancellationToken token);
}
public sealed record SavePresentationCommand(Guid ResourceId, bool Update, string IdempotencyKey, SaveCommercialPresentation Input) : ITransactionalCommand<CommercialPresentationAggregate?>;
public sealed record GetPresentationQuery(Guid Id) : IQuery<CommercialPresentationAggregate?>;
public sealed record ListPresentationsQuery(Guid OpportunityId, int Page = 1, int PageSize = 50) : IQuery<PagedResult<CommercialPresentationAggregate>?>;
public sealed class PresentationHandlers(ICanonicalPresentationStore store) :
    IRequestHandler<SavePresentationCommand, CommercialPresentationAggregate?>,
    IRequestHandler<GetPresentationQuery, CommercialPresentationAggregate?>,
    IRequestHandler<ListPresentationsQuery, PagedResult<CommercialPresentationAggregate>?>
{
    public Task<CommercialPresentationAggregate?> Handle(SavePresentationCommand r, CancellationToken t) => store.SaveAsync(r.ResourceId, r.Update, r.IdempotencyKey, r.Input, t);
    public Task<CommercialPresentationAggregate?> Handle(GetPresentationQuery r, CancellationToken t) => store.GetAsync(r.Id, t);
    public Task<PagedResult<CommercialPresentationAggregate>?> Handle(ListPresentationsQuery r, CancellationToken t) => store.ListAsync(r.OpportunityId, Math.Max(1, r.Page), Math.Clamp(r.PageSize, 1, 100), t);
}
public sealed class SavePresentationCommandValidator : AbstractValidator<SavePresentationCommand>
{
    public SavePresentationCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty(); RuleFor(x => x.IdempotencyKey).Must(ExecutionValidation.ValidKey);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.ExpectedRevision).Must((command, revision) => command.Update ? revision is > 0 : revision is null);
            RuleFor(x => x.Input.Layout).Must(x => x is "VERTICAL" or "HORIZONTAL");
            RuleFor(x => x.Input.Ordering).Must(x => x is "RECOMENDACAO" or "PREMIO" or "FRANQUIA" or "MANUAL");
            RuleFor(x => x.Input.Status).Must(x => x is "RASCUNHO" or "GERADA");
            RuleFor(x => x.Input.Title).MaximumLength(200);
            RuleFor(x => x.Input.CommercialNotes).MaximumLength(4000);
            RuleFor(x => x.Input.Items).NotNull().Must(x => x is { Count: <= 5 });
            RuleFor(x => x.Input.Items).Must((c, items) => items is null || items.All(x => x is not null)
                && items.Select(x => x.QuoteId).Distinct().Count() == items.Count
                && (c.Input.Status != "GERADA" || items.Count > 0)
                && (c.Input.SelectedQuoteId is null || items.Any(x => x.QuoteId == c.Input.SelectedQuoteId)));
            RuleForEach(x => x.Input.Items).NotNull().SetValidator(new PresentationQuoteInputValidator());
        });
    }
}
public sealed class PresentationQuoteInputValidator : AbstractValidator<PresentationQuoteInput>
{
    public PresentationQuoteInputValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty(); RuleFor(x => x.InstallmentId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.CommercialTitle).MaximumLength(200); RuleFor(x => x.Advantages).MaximumLength(4000);
        RuleFor(x => x.CommercialNote).MaximumLength(4000);
    }
}
