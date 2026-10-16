using System.Text.Json.Serialization;

namespace WAssis.Application.Modules.Calculations;

public sealed record CalculationInsurerOption
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("tenant_id")] public Guid TenantId { get; init; }
    [JsonPropertyName("nome")] public string Name { get; init; } = string.Empty;
}
public sealed record CalculationCoverageOption
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("ramo_id")] public Guid InsuranceBranchId { get; init; }
    [JsonPropertyName("codigo")] public string? Code { get; init; }
    [JsonPropertyName("codigo_susep")] public string? SusepCode { get; init; }
    [JsonPropertyName("nome")] public string? Name { get; init; }
    [JsonPropertyName("descricao")] public string? Description { get; init; }
    [JsonPropertyName("tipo_cobertura")] public string? CoverageType { get; init; }
    [JsonPropertyName("caracteristica")] public string? Characteristic { get; init; }
    [JsonPropertyName("tipo_risco")] public string? RiskType { get; init; }
    [JsonPropertyName("modalidade")] public string? Modality { get; init; }
    [JsonPropertyName("capital_lmi_padrao")] public decimal? DefaultLimit { get; init; }
    [JsonPropertyName("franquia_padrao")] public decimal? DefaultDeductible { get; init; }
    [JsonPropertyName("carencia_dias")] public int? WaitingDays { get; init; }
    [JsonPropertyName("obrigatoria")] public bool? Required { get; init; }
    [JsonPropertyName("ordem")] public int? Order { get; init; }
    [JsonPropertyName("ativo")] public bool? Active { get; init; }
}
public sealed record CalculationSourcePolicy(Guid Id, Guid InsuranceBranchId, Guid InsuredPersonId,
    Guid? InsurerId, string? Number, string? CoverageEndsOn, string? Status);
public sealed record CalculationEditorContext(Guid OpportunityId, Guid? InsuranceBranchId, Guid? InsuredPersonId,
    string? LeadName, Guid BranchId, string TimeZone, string BranchDate, CalculationSourcePolicy? SourcePolicy);
