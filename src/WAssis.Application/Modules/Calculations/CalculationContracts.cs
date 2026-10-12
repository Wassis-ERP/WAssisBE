using System.Text.Json.Serialization;

namespace WAssis.Application.Modules.Calculations;

public record CalculationFields
{
    [JsonPropertyName("oportunidade_id")] public Guid OpportunityId { get; init; }
    [JsonPropertyName("ramo_id")] public Guid InsuranceBranchId { get; init; }
    [JsonPropertyName("segurado_id")] public Guid? InsuredPersonId { get; init; }
    [JsonPropertyName("seguradora_anterior_id")] public Guid? PreviousInsurerId { get; init; }
    [JsonPropertyName("origem")] public string Origin { get; init; } = "MANUAL";
    [JsonPropertyName("comissao_sugerida_pct")] public decimal? SuggestedCommission { get; init; }
    [JsonPropertyName("tipo_seguro")] public string InsuranceType { get; init; } = "NOVO";
    [JsonPropertyName("bonus")] public int? Bonus { get; init; }
    [JsonPropertyName("qtd_sinistros")] public int? ClaimCount { get; init; }
    [JsonPropertyName("qtd_sinistros_perda_parcial")] public int? PartialLossClaimCount { get; init; }
    [JsonPropertyName("transferiu_titularidade")] public bool? OwnershipTransferred { get; init; }
    [JsonPropertyName("vigencia_inicio")] public string? CoverageStartsOn { get; init; }
    [JsonPropertyName("vigencia_fim")] public string? CoverageEndsOn { get; init; }
    [JsonPropertyName("vigencia_fim_anterior")] public string? PreviousCoverageEndsOn { get; init; }
    [JsonPropertyName("numero_apolice_anterior")] public string? PreviousPolicyNumber { get; init; }
    [JsonPropertyName("codigo_identificacao_anterior")] public string? PreviousIdentification { get; init; }
    [JsonPropertyName("status_apolice_anterior")] public string? PreviousPolicyStatus { get; init; }
    [JsonPropertyName("nota_interna")] public string? InternalNote { get; init; }
}
public sealed record CreateAutoCalculation : CalculationFields
{
    public AutoCalculationSpecialization Specialization { get; init; } = new();
    public IReadOnlyList<RequestedCalculationCoverage> Coverages { get; init; } = [];
}
public sealed record CalculationRow : CalculationFields
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("rotulo_versao")] public string VersionLabel { get; init; } = string.Empty;
    [JsonPropertyName("criado_em")] public DateTime CreatedAtUtc { get; init; }
}
public sealed record AutoCalculationSpecialization
{
    [JsonPropertyName("forma")] public string Form { get; init; } = "AUTO";
    [JsonPropertyName("data")] public AutoCalculationRisk Data { get; init; } = new();
}
public sealed record AutoCalculationRisk
{
    [JsonPropertyName("calculo_id")] public Guid CalculationId { get; init; }
    [JsonPropertyName("codigo_fipe")] public string? FipeCode { get; init; }
    [JsonPropertyName("marca")] public string? Brand { get; init; }
    [JsonPropertyName("modelo")] public string? Model { get; init; }
    [JsonPropertyName("versao")] public string? Version { get; init; }
    [JsonPropertyName("ano_fabricacao")] public int? ManufactureYear { get; init; }
    [JsonPropertyName("ano_modelo")] public int? ModelYear { get; init; }
    [JsonPropertyName("placa")] public string? Plate { get; init; }
    [JsonPropertyName("chassi")] public string? Chassis { get; init; }
    [JsonPropertyName("chassi_remarcado")] public bool? ChassisRemarked { get; init; }
    [JsonPropertyName("renavam")] public string? Renavam { get; init; }
    [JsonPropertyName("zero_km")] public bool? ZeroKm { get; init; }
    [JsonPropertyName("combustivel")] public string? Fuel { get; init; }
    [JsonPropertyName("cambio")] public string? Transmission { get; init; }
    [JsonPropertyName("categoria")] public string? Category { get; init; }
    [JsonPropertyName("tipo_veiculo")] public string? VehicleType { get; init; }
    [JsonPropertyName("uso")] public string? Usage { get; init; }
    [JsonPropertyName("cep_pernoite")] public string? OvernightPostalCode { get; init; }
    [JsonPropertyName("possui_garagem_residencia")] public bool? ResidenceGarage { get; init; }
    [JsonPropertyName("possui_garagem_trabalho")] public bool? WorkGarage { get; init; }
    [JsonPropertyName("possui_garagem_estudo")] public bool? StudyGarage { get; init; }
    [JsonPropertyName("km_mensal")] public int? MonthlyKilometres { get; init; }
    [JsonPropertyName("blindado")] public bool? Armored { get; init; }
    [JsonPropertyName("alienado")] public bool? Financed { get; init; }
    [JsonPropertyName("rastreador")] public bool? Tracker { get; init; }
    [JsonPropertyName("antifurto")] public bool? AntiTheft { get; init; }
    [JsonPropertyName("kit_gas")] public bool? GasKit { get; init; }
    [JsonPropertyName("condutor_nome")] public string? DriverName { get; init; }
    [JsonPropertyName("condutor_cpf")] public string? DriverDocument { get; init; }
    [JsonPropertyName("condutor_data_nascimento")] public string? DriverBirthDate { get; init; }
    [JsonPropertyName("condutor_sexo")] public string? DriverGender { get; init; }
    [JsonPropertyName("condutor_estado_civil")] public string? DriverMaritalStatus { get; init; }
    [JsonPropertyName("condutor_profissao")] public string? DriverProfession { get; init; }
    [JsonPropertyName("condutor_reside_com_segurado")] public bool? DriverLivesWithInsured { get; init; }
    [JsonPropertyName("condutor_tempo_habilitacao")] public int? DriverLicenseYears { get; init; }
}
public record RequestedCalculationCoverage
{
    [JsonPropertyName("cobertura_id")] public Guid CoverageId { get; init; }
    [JsonPropertyName("selecionada")] public bool? Selected { get; init; }
    [JsonPropertyName("limite_solicitado")] public decimal? RequestedLimit { get; init; }
    [JsonPropertyName("percentual_fipe_solicitado")] public decimal? RequestedFipePercentage { get; init; }
    [JsonPropertyName("franquia_tipo_solicitado")] public string? RequestedDeductibleType { get; init; }
    [JsonPropertyName("opcao_solicitada")] public string? RequestedOption { get; init; }
    [JsonPropertyName("quantidade_solicitada")] public int? RequestedQuantity { get; init; }
    [JsonPropertyName("observacao_transmitida")] public string? TransmittedObservation { get; init; }
}
public sealed record CalculationCoverageRow : RequestedCalculationCoverage
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("calculo_id")] public Guid CalculationId { get; init; }
}
public sealed record CalculationAggregate(CalculationRow Calculation, AutoCalculationSpecialization Specialization,
    IReadOnlyCollection<CalculationCoverageRow> Coverages);
public sealed class CalculationConflictException : Exception;

