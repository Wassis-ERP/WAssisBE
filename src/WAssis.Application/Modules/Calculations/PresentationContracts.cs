using System.Text.Json.Serialization;

namespace WAssis.Application.Modules.Calculations;

public record PresentationFields
{
    [JsonPropertyName("cotacao_escolhida_id")] public Guid? SelectedQuoteId { get; init; }
    [JsonPropertyName("titulo")] public string? Title { get; init; }
    [JsonPropertyName("layout")] public string Layout { get; init; } = "VERTICAL";
    [JsonPropertyName("criterio_ordenacao")] public string Ordering { get; init; } = "MANUAL";
    [JsonPropertyName("exibir_percentual_fipe")] public bool ShowFipe { get; init; } = true;
    [JsonPropertyName("exibir_vantagens")] public bool ShowAdvantages { get; init; } = true;
    [JsonPropertyName("exibir_legenda")] public bool ShowLegend { get; init; } = true;
    [JsonPropertyName("exibir_observacoes")] public bool ShowNotes { get; init; } = true;
    [JsonPropertyName("exibir_comissao")] public bool ShowCommission { get; init; }
    [JsonPropertyName("observacoes_comerciais")] public string? CommercialNotes { get; init; }
    [JsonPropertyName("status")] public string Status { get; init; } = "RASCUNHO";
}
public sealed record SaveCommercialPresentation : PresentationFields
{
    public int? ExpectedRevision { get; init; }
    public IReadOnlyList<PresentationQuoteInput> Items { get; init; } = [];
}
public record PresentationQuoteInput
{
    [JsonPropertyName("cotacao_id")] public Guid QuoteId { get; init; }
    [JsonPropertyName("parcelamento_id")] public Guid? InstallmentId { get; init; }
    [JsonPropertyName("recomendada")] public bool Recommended { get; init; }
    [JsonPropertyName("titulo_comercial")] public string? CommercialTitle { get; init; }
    [JsonPropertyName("vantagens_texto")] public string? Advantages { get; init; }
    [JsonPropertyName("observacao_comercial")] public string? CommercialNote { get; init; }
}
public sealed record PresentationQuoteRow : PresentationQuoteInput
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("apresentacao_id")] public Guid PresentationId { get; init; }
    [JsonPropertyName("ordem")] public int Order { get; init; }
}
public sealed record CommercialPresentationRow : PresentationFields
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("oportunidade_id")] public Guid OpportunityId { get; init; }
    [JsonPropertyName("criado_por_id")] public Guid? CreatedById { get; init; }
    [JsonPropertyName("criado_em")] public DateTime CreatedAtUtc { get; init; }
    [JsonPropertyName("atualizado_em")] public DateTime UpdatedAtUtc { get; init; }
    [JsonPropertyName("gerada_em")] public DateTime? GeneratedAtUtc { get; init; }
    [JsonPropertyName("escolhida_em")] public DateTime? SelectedAtUtc { get; init; }
}
public sealed record PresentationResult(QuoteResultAggregate Result, CalculationRow Calculation, string InsurerName);
public sealed record CommercialPresentationAggregate(CommercialPresentationRow Presentation,
    IReadOnlyCollection<PresentationQuoteRow> Items, int Revision, IReadOnlyCollection<PresentationResult> Results);
public sealed class PresentationConflictException : Exception;
