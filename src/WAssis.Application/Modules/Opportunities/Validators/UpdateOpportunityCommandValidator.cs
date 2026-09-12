using FluentValidation;
using WAssis.Application.Modules.Opportunities.Commands;

namespace WAssis.Application.Modules.Opportunities.Validators;

public sealed class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OfficeBranchId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.StageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ResponsibleId).MaximumLength(64);
        RuleFor(x => x.Status)
            .MaximumLength(32)
            .Must(BeSupportedStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
        RuleFor(x => x.BusinessType).MaximumLength(32);
        RuleFor(x => x.LeadName).MaximumLength(200);
        RuleFor(x => x.LeadDocumentNumber).MaximumLength(32);
        RuleFor(x => x.LeadEmail).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.LeadEmail));
        RuleFor(x => x.LeadPhoneNumber).MaximumLength(32);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).MaximumLength(32);
        RuleFor(x => x.EstimatedPremiumAmount).GreaterThanOrEqualTo(0).When(x => x.EstimatedPremiumAmount.HasValue);
        RuleFor(x => x.EstimatedCommissionAmount).GreaterThanOrEqualTo(0).When(x => x.EstimatedCommissionAmount.HasValue);
        RuleFor(x => x).Must(x => !x.OpenedOn.HasValue || !x.ExpectedCloseDate.HasValue || x.ExpectedCloseDate >= x.OpenedOn)
            .WithMessage("ExpectedCloseDate cannot precede OpenedOn.");
        RuleFor(x => x).Must(x => !x.WonAtUtc.HasValue || !x.LostAtUtc.HasValue)
            .WithMessage("WonAtUtc and LostAtUtc cannot both be set.");
        RuleFor(x => x).Must(x => !string.Equals(x.Status, "won", StringComparison.OrdinalIgnoreCase) || x.ConcludedAtUtc.HasValue || x.WonAtUtc.HasValue)
            .WithMessage("Won opportunities must include ConcludedAtUtc or WonAtUtc.");
        RuleFor(x => x).Must(x => !string.Equals(x.Status, "lost", StringComparison.OrdinalIgnoreCase) || x.ConcludedAtUtc.HasValue || x.LostAtUtc.HasValue)
            .WithMessage("Lost opportunities must include ConcludedAtUtc or LostAtUtc.");
        RuleFor(x => x.MetadataJson)
            .Must(BeJsonObject)
            .WithMessage("Metadata must be a JSON object.");
    }

    private static bool BeSupportedStatus(string? status)
    {
        return status is not null && new[] { "pending", "won", "lost" }.Contains(status, StringComparer.OrdinalIgnoreCase);
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
