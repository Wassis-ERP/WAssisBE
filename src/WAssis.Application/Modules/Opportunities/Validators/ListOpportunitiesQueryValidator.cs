using FluentValidation;
using WAssis.Application.Modules.Opportunities.Queries;

namespace WAssis.Application.Modules.Opportunities.Validators;

public sealed class ListOpportunitiesQueryValidator : AbstractValidator<ListOpportunitiesQuery>
{
    public ListOpportunitiesQueryValidator()
    {
        RuleFor(x => x.PipelineId).Must(ValidUuid);
        RuleFor(x => x.StageId).Must(ValidUuid);
        RuleFor(x => x.Status).Must(x => string.IsNullOrWhiteSpace(x) ||
            new[] { "pending", "won", "lost" }.Contains(x.Trim().ToLowerInvariant()));
    }
    private static bool ValidUuid(string? value) => string.IsNullOrWhiteSpace(value) ||
        Guid.TryParse(value, out var id) && id != Guid.Empty;
}
