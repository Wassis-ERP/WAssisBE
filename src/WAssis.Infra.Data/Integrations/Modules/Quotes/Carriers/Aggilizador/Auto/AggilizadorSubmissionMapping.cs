using System.Globalization;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Validators;
using WAssis.Domain.Modules.Quotes.ValueObjects;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto;

internal static class AggilizadorSubmissionMapping
{
    private static readonly Dictionary<string, string> AmountFields = new(StringComparer.Ordinal)
    {
        ["danos-morais"] = "DanosMorais", ["danos-materiais"] = "DanosMateriais",
        ["danos-corporais"] = "DanosCorporais", ["morte-invalidez-passageiros"] = "MorteInvalidez",
        ["kit-gas"] = "KitGas", ["blindagem"] = "Blindagem", ["carroceria"] = "Carroceria",
        ["equipamentos"] = "Equipamento"
    };
    private static readonly Dictionary<string, string> OptionFields = new(StringComparer.Ordinal)
    {
        ["assistencia-24h"] = "Assistencia", ["vidros"] = "Vidros", ["carro-reserva"] = "CarroReserva"
    };

    public static IReadOnlyCollection<QuoteStatusMessageDto> Validate(AutoQuoteSubmission? submission, AggilizadorAutoQuoteOptions options)
    {
        if (submission is null)
            return [new("missing_auto_submission", "Informe o snapshot de condutor, vigência e coberturas antes de transmitir.")];
        if (!new AutoQuoteSubmissionValidator().Validate(submission).IsValid)
            return [new("invalid_auto_submission", "Revise os dados do snapshot de transmissão Auto.")];

        var errors = new List<QuoteStatusMessageDto>();
        if (!Mapped(options.InsuranceTypeCodes, submission.InsuranceType))
            errors.Add(new("unmapped_insurance_type", "O tipo de seguro exige de-para validado para o Aggilizador."));
        if (!Mapped(options.DriverMaritalStatusCodes, submission.DriverMaritalStatus))
            errors.Add(new("unmapped_driver_marital_status", "O estado civil do condutor exige de-para validado."));
        // Own-renewal provenance must be derived from the canonical policy, not trusted from an envelope.
        if (submission.InsuranceType == "RENOVACAO_PROPRIA")
            errors.Add(new("source_policy_verification_required", "A renovação própria depende da validação da apólice de origem no fluxo canônico."));
        var casco = submission.Coverages.SingleOrDefault(c => c.Code == "casco");
        if (casco is not { Selected: true, RequestedFipePercentage: not null }
            || !Mapped(options.DeductibleTypeCodes, casco.RequestedDeductibleType))
            errors.Add(new("unsupported_casco_request", "Informe casco com percentual FIPE e tipo de franquia com de-para validado."));
        foreach (var coverage in submission.Coverages.Where(c => c.Selected))
        {
            bool supported = coverage.Code == "casco"
                ? coverage.RequestedLimit is null && coverage.RequestedOption is null && coverage.RequestedQuantity is null
                : AmountFields.ContainsKey(coverage.Code)
                    ? coverage.RequestedLimit.HasValue && coverage.RequestedFipePercentage is null
                        && coverage.RequestedDeductibleType is null && coverage.RequestedOption is null && coverage.RequestedQuantity is null
                    : OptionFields.ContainsKey(coverage.Code)
                        && Mapped(options.CoverageOptionCodes, $"{coverage.Code}:{coverage.RequestedOption}")
                        && coverage.RequestedLimit is null && coverage.RequestedFipePercentage is null
                        && coverage.RequestedDeductibleType is null && coverage.RequestedQuantity is null;
            if (!supported)
                errors.Add(new("unsupported_coverage_request", "Uma preferência de cobertura não possui tradução completa para o Aggilizador."));
        }
        return errors;
    }

    public static Dictionary<string, string> BuildCoverages(AutoQuoteSubmission submission, AggilizadorAutoQuoteOptions options)
    {
        var result = AmountFields.Values.ToDictionary(field => field, _ => "0.00");
        foreach (var field in OptionFields.Values) result[field] = "0";
        foreach (var coverage in submission.Coverages.Where(c => c.Selected))
        {
            if (AmountFields.TryGetValue(coverage.Code, out var amountField))
                result[amountField] = coverage.RequestedLimit!.Value.ToString("0.00", CultureInfo.InvariantCulture);
            else if (OptionFields.TryGetValue(coverage.Code, out var optionField))
                result[optionField] = options.CoverageOptionCodes[$"{coverage.Code}:{coverage.RequestedOption}"];
        }
        var casco = submission.Coverages.Single(c => c.Code == "casco");
        result["FatorAjuste"] = casco.RequestedFipePercentage!.Value.ToString(CultureInfo.InvariantCulture);
        result["TipoFranquia"] = options.DeductibleTypeCodes[casco.RequestedDeductibleType!];
        result["ArCondicionado"] = "0";
        return result;
    }

    private static bool Mapped(IReadOnlyDictionary<string, string> map, string? key) =>
        key is not null && map.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);
}
