namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record InstallmentSnapshotDto(
    int Number,
    decimal Amount,
    decimal? TotalAmount);
