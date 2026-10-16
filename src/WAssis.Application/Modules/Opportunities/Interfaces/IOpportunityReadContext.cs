using System.Text.Json.Serialization;

namespace WAssis.Application.Modules.Opportunities.Interfaces;

public sealed record OpportunityReferenceLabel
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("nome")] public string? Name { get; init; }
}
public sealed record OpportunityInsuranceBranchLabel
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("nome")] public string? Name { get; init; }
    [JsonPropertyName("risk_type")] public string? RiskType { get; init; }
    [JsonPropertyName("grupo_operacional")] public string? OperationalGroup { get; init; }
    [JsonPropertyName("forma_calculo")] public string? CalculationForm { get; init; }
}
public sealed record OpportunityStageLabel
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("pipeline_id")] public Guid PipelineId { get; init; }
    [JsonPropertyName("nome")] public string? Name { get; init; }
    [JsonPropertyName("codigo")] public string? Code { get; init; }
    [JsonPropertyName("tipo_stage")] public string? Type { get; init; }
    [JsonPropertyName("cor")] public string? Color { get; init; }
    [JsonPropertyName("ordem")] public int? Order { get; init; }
    [JsonPropertyName("probabilidade")] public decimal? Probability { get; init; }
    [JsonPropertyName("sla_dias")] public int? SlaDays { get; init; }
    [JsonPropertyName("finaliza_com_sucesso")] public bool? CanWin { get; init; }
    [JsonPropertyName("finaliza_com_perda")] public bool? CanLose { get; init; }
    [JsonPropertyName("ativo")] public bool? Active { get; init; }
}
public sealed record OpportunityReadContext(Guid OpportunityId, OpportunityInsuranceBranchLabel? InsuranceBranch,
    OpportunityReferenceLabel? Origin, OpportunityReferenceLabel? LossReason, OpportunityStageLabel? Stage);

public interface IOpportunityReadContext
{
    Task<OpportunityReadContext?> GetAsync(Guid opportunityId, CancellationToken cancellationToken);
}
