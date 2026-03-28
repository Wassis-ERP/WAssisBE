using System.Globalization;
using System.Text.RegularExpressions;
using WAssis.Application.Modules.Financial.Dtos;
using WAssis.Application.Modules.Financial.Interfaces;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class CommissionStatementParser : ICommissionStatementParser
{
    public CommissionStatementAnalysisDto Analyze(string sourceType, string rawText)
    {
        var lines = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((line, index) => ParseLine(index + 1, line))
            .Where(static line => !string.IsNullOrWhiteSpace(line.RawText))
            .ToArray();

        return new CommissionStatementAnalysisDto(
            sourceType,
            lines.Length,
            lines.Count(static line => line.NeedsManualReview),
            lines);
    }

    private static CommissionStatementLineDto ParseLine(int lineNumber, string line)
    {
        var amount = ExtractMoney(line);
        var proposalNumber = MatchValue(line, @"(?:proposta|apolice|ref|referencia)[:\s#-]+(?<value>[A-Za-z0-9\-/\.]+)");
        var company = MatchValue(line, @"(?:seguradora|cia|empresa)[:\s]+(?<value>.*?)(?=\s+(?:proposta|apolice|ref|referencia|valor)\b|$)");
        var occurredAtUtc = ExtractDate(line);
        var suggestedReference = proposalNumber ?? MatchValue(line, @"(?<value>[A-Z]{2,}-\d{2,})");
        var needsManualReview = amount is null || string.IsNullOrWhiteSpace(suggestedReference);

        return new CommissionStatementLineDto(
            lineNumber,
            line,
            company,
            proposalNumber,
            amount,
            occurredAtUtc,
            suggestedReference,
            needsManualReview);
    }

    private static string? MatchValue(string line, string pattern)
    {
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value.Trim() : null;
    }

    private static decimal? ExtractMoney(string line)
    {
        var match = Regex.Match(line, @"(?<value>\d{1,3}(?:\.\d{3})*,\d{2})", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return null;
        }

        var normalized = match.Groups["value"].Value.Replace(".", string.Empty).Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static DateTime? ExtractDate(string line)
    {
        var match = Regex.Match(line, @"(?<value>\d{2}/\d{2}/\d{4})", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return null;
        }

        return DateTime.TryParseExact(
            match.Groups["value"].Value,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var date)
            ? date.ToUniversalTime()
            : null;
    }
}
