namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record CoverageSnapshotDto(
    string Code,
    string Name,
    decimal? InsuredAmount,
    decimal? DeductibleAmount);
