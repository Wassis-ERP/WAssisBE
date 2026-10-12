using System.Text.Json.Serialization;

namespace WAssis.Application.Modules.Calculations;

public sealed record CreateCalculationExecution
{
    [JsonPropertyName("seguradora_id")] public Guid InsurerId { get; init; }
    [JsonPropertyName("motor")] public string Engine { get; init; } = "MANUAL";
    [JsonPropertyName("comissao_pct_sobrescrita")] public decimal? CommissionOverride { get; init; }
}
public sealed record RetryCalculationExecution
{
    [JsonPropertyName("comissao_pct_sobrescrita")] public decimal? CommissionOverride { get; init; }
}
public sealed record CalculationExecutionRow
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("calculo_id")] public Guid CalculationId { get; init; }
    [JsonPropertyName("seguradora_id")] public Guid InsurerId { get; init; }
    [JsonPropertyName("reexecucao_de_id")] public Guid? RetryOfId { get; init; }
    [JsonPropertyName("tentativa")] public int Attempt { get; init; }
    [JsonPropertyName("motor")] public string Engine { get; init; } = "MANUAL";
    [JsonPropertyName("comissao_pct_aplicada")] public decimal? AppliedCommission { get; init; }
    [JsonPropertyName("comissao_origem")] public string CommissionOrigin { get; init; } = "PADRAO_CALCULO";
    [JsonPropertyName("status")] public string Status { get; init; } = "AGUARDANDO";
    [JsonPropertyName("pendencia_codigo")] public string? PendingCode { get; init; }
    [JsonPropertyName("pendencia_mensagem")] public string? PendingMessage { get; init; }
    [JsonPropertyName("erro_codigo")] public string? ErrorCode { get; init; }
    [JsonPropertyName("erro_mensagem_segura")] public string? SafeErrorMessage { get; init; }
    [JsonPropertyName("referencia_externa")] public string? ExternalReference { get; init; }
    [JsonPropertyName("iniciada_em")] public DateTime? StartedAtUtc { get; init; }
    [JsonPropertyName("concluida_em")] public DateTime? FinishedAtUtc { get; init; }
    [JsonPropertyName("criada_em")] public DateTime CreatedAtUtc { get; init; }
}
public record QuoteResultFields
{
    [JsonPropertyName("numero_cotacao_seguradora")] public string? InsurerQuoteNumber { get; init; }
    [JsonPropertyName("premio_total")] public decimal? TotalPremium { get; init; }
    [JsonPropertyName("premio_liquido")] public decimal? NetPremium { get; init; }
    [JsonPropertyName("iof")] public decimal? Tax { get; init; }
    [JsonPropertyName("adicional_fracionamento")] public decimal? InstallmentSurcharge { get; init; }
    [JsonPropertyName("comissao_valor")] public decimal? CommissionAmount { get; init; }
    [JsonPropertyName("validade")] public string? ValidUntil { get; init; }
    [JsonPropertyName("link_proposta")] public string? ProposalLink { get; init; }
    [JsonPropertyName("mensagem_seguradora")] public string? InsurerMessage { get; init; }
    [JsonPropertyName("restricoes")] public string? Restrictions { get; init; }
    [JsonPropertyName("observacao_interna")] public string? InternalObservation { get; init; }
}
public sealed record RecordManualQuote : QuoteResultFields
{
    public IReadOnlyList<ReturnedQuoteCoverage> Coverages { get; init; } = [];
    public IReadOnlyList<QuoteInstallmentOption> Installments { get; init; } = [];
}
public sealed record QuoteResultRow : QuoteResultFields
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("execucao_id")] public Guid ExecutionId { get; init; }
    [JsonPropertyName("status")] public string Status { get; init; } = "APRESENTADA";
    [JsonPropertyName("recebida_em")] public DateTime ReceivedAtUtc { get; init; }
    [JsonPropertyName("aprovada_em")] public DateTime? ApprovedAtUtc { get; init; }
    [JsonPropertyName("descartada_motivo")] public string? DiscardReason { get; init; }
}
public record ReturnedQuoteCoverage
{
    [JsonPropertyName("cobertura_id")] public Guid? CoverageId { get; init; }
    [JsonPropertyName("chave_resultado")] public string ResultKey { get; init; } = string.Empty;
    [JsonPropertyName("codigo_externo")] public string? ExternalCode { get; init; }
    [JsonPropertyName("nome_informado")] public string? ReportedName { get; init; }
    [JsonPropertyName("incluida")] public bool? Included { get; init; }
    [JsonPropertyName("limite_aceito")] public decimal? AcceptedLimit { get; init; }
    [JsonPropertyName("percentual_fipe_aceito")] public decimal? AcceptedFipePercentage { get; init; }
    [JsonPropertyName("franquia_tipo")] public string? DeductibleType { get; init; }
    [JsonPropertyName("franquia_valor")] public decimal? DeductibleAmount { get; init; }
    [JsonPropertyName("premio")] public decimal? Premium { get; init; }
    [JsonPropertyName("carencia_dias")] public int? WaitingDays { get; init; }
    [JsonPropertyName("participacao_obrigatoria_pct")] public decimal? MandatoryParticipation { get; init; }
    [JsonPropertyName("clausula_texto")] public string? ClauseText { get; init; }
    [JsonPropertyName("observacao_seguradora")] public string? InsurerObservation { get; init; }
    [JsonPropertyName("ordem")] public int? Order { get; init; }
}
public sealed record QuoteCoverageRow : ReturnedQuoteCoverage
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("cotacao_id")] public Guid QuoteId { get; init; }
}
public record QuoteInstallmentOption
{
    [JsonPropertyName("codigo_opcao")] public string OptionCode { get; init; } = string.Empty;
    [JsonPropertyName("forma_pagamento")] public string PaymentMethod { get; init; } = string.Empty;
    [JsonPropertyName("quantidade_parcelas")] public int InstallmentCount { get; init; }
    [JsonPropertyName("valor_entrada")] public decimal? DownPayment { get; init; }
    [JsonPropertyName("valor_parcela")] public decimal? InstallmentAmount { get; init; }
    [JsonPropertyName("valor_total")] public decimal? TotalAmount { get; init; }
    [JsonPropertyName("juros_pct")] public decimal? InterestPercentage { get; init; }
    [JsonPropertyName("adicional_fracionamento")] public decimal? InstallmentSurcharge { get; init; }
    [JsonPropertyName("primeiro_vencimento")] public string? FirstDueDate { get; init; }
    [JsonPropertyName("principal")] public bool? Primary { get; init; }
    [JsonPropertyName("ordem")] public int? Order { get; init; }
}
public sealed record QuoteInstallmentRow : QuoteInstallmentOption
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("cotacao_id")] public Guid QuoteId { get; init; }
}
public sealed record QuoteResultAggregate(QuoteResultRow Quote, CalculationExecutionRow Execution,
    IReadOnlyCollection<QuoteCoverageRow> Coverages, IReadOnlyCollection<QuoteInstallmentRow> Installments);
public sealed class ExecutionStateConflictException : Exception;
