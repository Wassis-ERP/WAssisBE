namespace WAssis.Infra.Data.Configuration;

public sealed class AggilizadorAutoQuoteOptions
{
    public const string SectionName = "Quotes:Providers:Aggilizador:Auto";

    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;
    public int InsuranceBrokerId { get; set; }
    public int PartnerId { get; set; }
    public int DefaultCommissionPercentage { get; set; } = 15;
    public string DefaultBankCode { get; set; } = "0";
    public string DefaultInsuranceTypeCode { get; set; } = "0";
    public string DefaultSinisterCode { get; set; } = "0";
    public string DefaultResidenceTypeCode { get; set; } = "1";
    public string DefaultVehiclesAtResidenceCode { get; set; } = "1";
    public string DefaultMonthlyMileageCode { get; set; } = "500";
    public string DefaultWorkGarageCode { get; set; } = "2";
    public string DefaultResidenceGarageCode { get; set; } = "1";
    public string DefaultStudyGarageCode { get; set; } = "0";
    public string DefaultVehicleUsageCode { get; set; } = "0";
    public string DefaultDependentVehicleUsageCode { get; set; } = "0";
    public string DefaultDependentAgeRangeCode { get; set; } = string.Empty;
    public string DefaultDistanceResidenceWorkCode { get; set; } = "1";
    public string DefaultProfessionCode { get; set; } = "342";
    public string DefaultPcdCode { get; set; } = "0";
    public string DefaultMoralDamagesAmount { get; set; } = "5000.00";
    public string DefaultMaterialDamagesAmount { get; set; } = "100000.00";
    public string DefaultBodilyDamagesAmount { get; set; } = "100000.00";
    public string DefaultDeathInvalidityAmount { get; set; } = "10000.00";
    public string DefaultDeductibleTypeCode { get; set; } = "1";
    public string DefaultAssistanceCode { get; set; } = "4";
    public string DefaultGlassCoverageCode { get; set; } = "1";
    public string DefaultReserveCarCode { get; set; } = "1";
    public string DefaultAdjustmentFactor { get; set; } = "100";
}
