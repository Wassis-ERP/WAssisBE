using FluentValidation;
using WAssis.Application.Modules.Opportunities.Commands;

namespace WAssis.Application.Modules.Opportunities.Validators;

public sealed class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OfficeBranchId).MaximumLength(64);
        RuleFor(x => x.ResponsibleId).MaximumLength(64);
        RuleFor(x => x.Status).MaximumLength(32);
        RuleFor(x => x.BusinessType).MaximumLength(32);
        RuleFor(x => x.MetadataJson)
            .Must(BeJsonObject)
            .WithMessage("Metadata must be a JSON object.");
    }

    private static bool BeJsonObject(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return true;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(metadataJson);
            return document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}
