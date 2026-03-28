using System.Globalization;
using System.Text.RegularExpressions;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class ProposalDocumentParser : IProposalDocumentParser
{
    public ProposalDocumentParsingResultDto Parse(string extractedText)
    {
        var normalizedText = Normalize(extractedText);
        var profile = DetectProfile(normalizedText);

        var insuranceCompanyName = profile?.CanonicalName ?? MatchValue(normalizedText, GenericInsuranceCompanyPatterns);
        var proposalNumber = MatchValue(normalizedText, MergePatterns(profile?.ProposalNumberPatterns, GenericProposalNumberPatterns));
        var insuredName = MatchValue(normalizedText, MergePatterns(profile?.InsuredNamePatterns, GenericInsuredNamePatterns));
        var coverageStartDateUtc = MatchDate(normalizedText, MergePatterns(profile?.CoverageStartPatterns, GenericCoverageStartPatterns));
        var coverageEndDateUtc = MatchDate(normalizedText, MergePatterns(profile?.CoverageEndPatterns, GenericCoverageEndPatterns));
        var totalPremiumAmount = MatchMoney(normalizedText, MergePatterns(profile?.PremiumPatterns, GenericPremiumPatterns));
        var commissionAmount = MatchMoney(normalizedText, MergePatterns(profile?.CommissionPatterns, GenericCommissionPatterns));

        var notes = new List<string>();
        if (profile is not null)
        {
            notes.Add($"Perfil de parser aplicado: {profile.CanonicalName}.");
        }

        if (string.IsNullOrWhiteSpace(insuranceCompanyName)) notes.Add("Seguradora não identificada.");
        if (string.IsNullOrWhiteSpace(proposalNumber)) notes.Add("Número da proposta não identificado.");
        if (string.IsNullOrWhiteSpace(insuredName)) notes.Add("Segurado não identificado.");
        if (coverageStartDateUtc is null || coverageEndDateUtc is null) notes.Add("Vigência incompleta.");
        if (totalPremiumAmount is null) notes.Add("Prêmio total não identificado.");

        return new ProposalDocumentParsingResultDto(
            "proposal_pdf",
            extractedText,
            insuranceCompanyName,
            proposalNumber,
            insuredName,
            coverageStartDateUtc,
            coverageEndDateUtc,
            totalPremiumAmount,
            commissionAmount,
            notes.Count == 0 ? "Parser inicial executado com sucesso." : string.Join(" ", notes));
    }

    private static string Normalize(string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static InsuranceProposalParserProfile? DetectProfile(string text)
    {
        return ParserProfiles.FirstOrDefault(profile =>
            profile.Aliases.Any(alias => text.Contains(alias, StringComparison.OrdinalIgnoreCase)));
    }

    private static IReadOnlyCollection<Regex> MergePatterns(IReadOnlyCollection<Regex>? primary, IReadOnlyCollection<Regex> fallback)
    {
        if (primary is null || primary.Count == 0)
        {
            return fallback;
        }

        return [.. primary, .. fallback];
    }

    private static string? MatchValue(string text, IReadOnlyCollection<Regex> patterns)
    {
        foreach (var pattern in patterns)
        {
            var match = pattern.Match(text);
            if (match.Success)
            {
                return match.Groups["value"].Value.Trim();
            }
        }

        return null;
    }

    private static DateTime? MatchDate(string text, IReadOnlyCollection<Regex> patterns)
    {
        var raw = MatchValue(text, patterns);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var acceptedFormats = new[]
        {
            "dd/MM/yyyy",
            "dd-MM-yyyy",
            "yyyy-MM-dd"
        };

        return DateTime.TryParseExact(raw, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date)
            ? date.ToUniversalTime()
            : null;
    }

    private static decimal? MatchMoney(string text, IReadOnlyCollection<Regex> patterns)
    {
        var raw = MatchValue(text, patterns);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var normalized = raw.Replace(".", string.Empty).Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static Regex NamedValueRegex(string pattern)
    {
        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    private sealed record InsuranceProposalParserProfile(
        string CanonicalName,
        IReadOnlyCollection<string> Aliases,
        IReadOnlyCollection<Regex>? ProposalNumberPatterns = null,
        IReadOnlyCollection<Regex>? InsuredNamePatterns = null,
        IReadOnlyCollection<Regex>? CoverageStartPatterns = null,
        IReadOnlyCollection<Regex>? CoverageEndPatterns = null,
        IReadOnlyCollection<Regex>? PremiumPatterns = null,
        IReadOnlyCollection<Regex>? CommissionPatterns = null);

    private static readonly Regex[] GenericInsuranceCompanyPatterns =
    [
        NamedValueRegex(@"Seguradora[:\s]+(?<value>[A-Za-zÀ-ÿ0-9 .\-]+?)(?:\s+(?:Número da proposta|Numero da proposta|Proposta|Vigência|Segurado|Cliente)|$)"),
        NamedValueRegex(@"Companhia[:\s]+(?<value>[A-Za-zÀ-ÿ0-9 .\-]+?)(?:\s+(?:Número da proposta|Numero da proposta|Proposta|Vigência|Segurado|Cliente)|$)")
    ];

    private static readonly Regex[] GenericProposalNumberPatterns =
    [
        NamedValueRegex(@"(?:Número da proposta|Numero da proposta|Proposta|Nº proposta|No proposta|Proposta nº|N\. da proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
    ];

    private static readonly Regex[] GenericInsuredNamePatterns =
    [
        NamedValueRegex(@"(?:Segurado|Nome do segurado|Cliente|Segurado\(a\))[:\s]+(?<value>[A-Za-zÀ-ÿ0-9 .'\-]+?)(?:\s+(?:CPF|CNPJ|Vigência|Veículo|Início|Fim|Das|Até|Prêmio|Valor)|$)")
    ];

    private static readonly Regex[] GenericCoverageStartPatterns =
    [
        NamedValueRegex(@"(?:Início de vigência|Vigência inicial|Início|Das)[:\s]+(?<value>\d{2}[/-]\d{2}[/-]\d{4})")
    ];

    private static readonly Regex[] GenericCoverageEndPatterns =
    [
        NamedValueRegex(@"(?:Fim de vigência|Vigência final|Fim|Até)[:\s]+(?<value>\d{2}[/-]\d{2}[/-]\d{4})")
    ];

    private static readonly Regex[] GenericPremiumPatterns =
    [
        NamedValueRegex(@"(?:Prêmio total|Prêmio líquido|Valor total|Total do seguro|Valor prêmio)[:\sR$]+(?<value>\d{1,3}(?:\.\d{3})*,\d{2})")
    ];

    private static readonly Regex[] GenericCommissionPatterns =
    [
        NamedValueRegex(@"(?:Comissão|Valor da comissão|Comissão total)[:\sR$]+(?<value>\d{1,3}(?:\.\d{3})*,\d{2})")
    ];

    private static readonly InsuranceProposalParserProfile[] ParserProfiles =
    [
        new(
            "Allianz",
            ["Allianz", "Allianz Seguros"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Proposta Allianz|Número da proposta|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ]),
        new(
            "Porto Seguro",
            ["Porto Seguro", "PORTO SEGURO", "PORTO"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Número da proposta|Proposta Porto|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ],
            PremiumPatterns:
            [
                NamedValueRegex(@"(?:Prêmio total|Valor total do prêmio|Total do prêmio)[:\sR$]+(?<value>\d{1,3}(?:\.\d{3})*,\d{2})")
            ]),
        new(
            "Tokio Marine",
            ["Tokio Marine", "TOKIO MARINE"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Número da proposta|Proposta Tokio|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ]),
        new(
            "Bradesco Seguros",
            ["Bradesco Seguros", "BRADESCO SEGUROS"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Número da proposta|Proposta Bradesco|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ]),
        new(
            "HDI Seguros",
            ["HDI", "HDI Seguros"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Número da proposta|Proposta HDI|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ]),
        new(
            "Azul Seguros",
            ["Azul Seguros", "AZUL SEGUROS"],
            ProposalNumberPatterns:
            [
                NamedValueRegex(@"(?:Número da proposta|Proposta Azul|Proposta)[:\s#]+(?<value>[A-Za-z0-9\-/\.]+)")
            ])
    ];
}
