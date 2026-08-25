using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Domain.Modules.Quotes.Entities;

public class QuoteRequest : AggregateRoot
{
    private readonly List<QuoteOption> _options = [];

    public string TenantId { get; private set; } = string.Empty;
    public string? OfficeBranchId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? InsuranceBranchId { get; private set; }
    public Guid? InsuredPersonId { get; private set; }
    public string CalculationType { get; private set; } = "AUTO";
    public string CalculationOrigin { get; private set; } = "PROPRIO";
    public string? VersionLabel { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CustomerSurname { get; private set; }
    public string? CustomerGender { get; private set; }
    public string? CustomerMaritalStatusCode { get; private set; }
    public DateTime? CustomerBirthDateUtc { get; private set; }
    public int? DriverLicenseYears { get; private set; }
    public string? DriverLicenseNumber { get; private set; }
    public string? InsuredDriverRelationshipCode { get; private set; }
    public string? VehicleChassisNumber { get; private set; }
    public string? VehiclePlate { get; private set; }
    public string? VehicleBrand { get; private set; }
    public string? VehicleModel { get; private set; }
    public string? VehicleFipeCode { get; private set; }
    public int? VehicleManufactureYear { get; private set; }
    public int VehicleModelYear { get; private set; }
    public bool? VehicleIsZeroKm { get; private set; }
    public bool? VehicleHasTracker { get; private set; }
    public bool? VehicleHasAntiTheft { get; private set; }
    public bool? VehicleIsFinanced { get; private set; }
    public bool? VehicleIsArmored { get; private set; }
    public string? VehicleFuelTypeCode { get; private set; }
    public string? VehicleOvernightPostalCode { get; private set; }
    public bool? VehicleHasKitGas { get; private set; }
    public bool HasDriverUnder24 { get; private set; }
    public bool IsCurrentlyInsured { get; private set; }
    public string? PreviousBonus { get; private set; }
    public int? BrokerCommissionPercentage { get; private set; }
    public int? RenewalInsurerCode { get; private set; }
    public QuoteRequestStatus Status { get; private set; }
    public string? ShareToken { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<QuoteOption> Options => _options;

    private QuoteRequest()
    {
    }

    private QuoteRequest(
        Guid id,
        string tenantId,
        string correlationId,
        string customerName,
        string documentNumber,
        string? email,
        string? phoneNumber,
        string? postalCode,
        string? customerSurname,
        string? customerGender,
        string? customerMaritalStatusCode,
        DateTime? customerBirthDateUtc,
        int? driverLicenseYears,
        string? driverLicenseNumber,
        string? insuredDriverRelationshipCode,
        string? vehicleChassisNumber,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        string? vehicleFipeCode,
        int? vehicleManufactureYear,
        int vehicleModelYear,
        bool? vehicleIsZeroKm,
        bool? vehicleHasTracker,
        bool? vehicleHasAntiTheft,
        bool? vehicleIsFinanced,
        bool? vehicleIsArmored,
        string? vehicleFuelTypeCode,
        string? vehicleOvernightPostalCode,
        bool? vehicleHasKitGas,
        bool hasDriverUnder24,
        bool isCurrentlyInsured,
        string? previousBonus,
        int? brokerCommissionPercentage,
        int? renewalInsurerCode,
        string? officeBranchId,
        Guid? opportunityId,
        Guid? insuranceBranchId,
        Guid? insuredPersonId,
        string calculationType,
        string calculationOrigin,
        string? versionLabel)
    {
        Id = id;
        TenantId = tenantId;
        OfficeBranchId = NormalizeOptional(officeBranchId);
        OpportunityId = opportunityId;
        InsuranceBranchId = insuranceBranchId;
        InsuredPersonId = insuredPersonId;
        CalculationType = NormalizeRequired(calculationType, "AUTO");
        CalculationOrigin = NormalizeRequired(calculationOrigin, "PROPRIO");
        VersionLabel = NormalizeOptional(versionLabel);
        CorrelationId = correlationId;
        CustomerName = customerName;
        DocumentNumber = documentNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        PostalCode = postalCode;
        CustomerSurname = customerSurname;
        CustomerGender = customerGender;
        CustomerMaritalStatusCode = customerMaritalStatusCode;
        CustomerBirthDateUtc = customerBirthDateUtc;
        DriverLicenseYears = driverLicenseYears;
        DriverLicenseNumber = driverLicenseNumber;
        InsuredDriverRelationshipCode = insuredDriverRelationshipCode;
        VehicleChassisNumber = vehicleChassisNumber;
        VehiclePlate = vehiclePlate;
        VehicleBrand = vehicleBrand;
        VehicleModel = vehicleModel;
        VehicleFipeCode = vehicleFipeCode;
        VehicleManufactureYear = vehicleManufactureYear;
        VehicleModelYear = vehicleModelYear;
        VehicleIsZeroKm = vehicleIsZeroKm;
        VehicleHasTracker = vehicleHasTracker;
        VehicleHasAntiTheft = vehicleHasAntiTheft;
        VehicleIsFinanced = vehicleIsFinanced;
        VehicleIsArmored = vehicleIsArmored;
        VehicleFuelTypeCode = vehicleFuelTypeCode;
        VehicleOvernightPostalCode = vehicleOvernightPostalCode;
        VehicleHasKitGas = vehicleHasKitGas;
        HasDriverUnder24 = hasDriverUnder24;
        IsCurrentlyInsured = isCurrentlyInsured;
        PreviousBonus = previousBonus;
        BrokerCommissionPercentage = brokerCommissionPercentage;
        RenewalInsurerCode = renewalInsurerCode;
        Status = QuoteRequestStatus.Pending;
        ShareToken = Guid.NewGuid().ToString("N");
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static QuoteRequest Create(
        string tenantId,
        string correlationId,
        string customerName,
        string documentNumber,
        string? email,
        string? phoneNumber,
        string? postalCode,
        string? customerSurname,
        string? customerGender,
        string? customerMaritalStatusCode,
        DateTime? customerBirthDateUtc,
        int? driverLicenseYears,
        string? driverLicenseNumber,
        string? insuredDriverRelationshipCode,
        string? vehicleChassisNumber,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        string? vehicleFipeCode,
        int? vehicleManufactureYear,
        int vehicleModelYear,
        bool? vehicleIsZeroKm,
        bool? vehicleHasTracker,
        bool? vehicleHasAntiTheft,
        bool? vehicleIsFinanced,
        bool? vehicleIsArmored,
        string? vehicleFuelTypeCode,
        string? vehicleOvernightPostalCode,
        bool? vehicleHasKitGas,
        bool hasDriverUnder24,
        bool isCurrentlyInsured,
        string? previousBonus,
        int? brokerCommissionPercentage,
        int? renewalInsurerCode,
        string? officeBranchId = null,
        Guid? opportunityId = null,
        Guid? insuranceBranchId = null,
        Guid? insuredPersonId = null,
        string calculationType = "AUTO",
        string calculationOrigin = "PROPRIO",
        string? versionLabel = null)
    {
        return new QuoteRequest(
            Guid.NewGuid(),
            tenantId,
            correlationId,
            customerName,
            documentNumber,
            email,
            phoneNumber,
            postalCode,
            customerSurname,
            customerGender,
            customerMaritalStatusCode,
            customerBirthDateUtc,
            driverLicenseYears,
            driverLicenseNumber,
            insuredDriverRelationshipCode,
            vehicleChassisNumber,
            vehiclePlate,
            vehicleBrand,
            vehicleModel,
            vehicleFipeCode,
            vehicleManufactureYear,
            vehicleModelYear,
            vehicleIsZeroKm,
            vehicleHasTracker,
            vehicleHasAntiTheft,
            vehicleIsFinanced,
            vehicleIsArmored,
            vehicleFuelTypeCode,
            vehicleOvernightPostalCode,
            vehicleHasKitGas,
            hasDriverUnder24,
            isCurrentlyInsured,
            previousBonus,
            brokerCommissionPercentage,
            renewalInsurerCode,
            officeBranchId,
            opportunityId,
            insuranceBranchId,
            insuredPersonId,
            calculationType,
            calculationOrigin,
            versionLabel);
    }

    private static string NormalizeRequired(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public void MarkAsProcessing()
    {
        Status = QuoteRequestStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddOption(QuoteOption option)
    {
        _options.Add(option);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = _options.Count == 0
            ? QuoteRequestStatus.Failed
            : _options.All(static option => option.Status == QuoteOptionStatus.Ok)
                ? QuoteRequestStatus.Completed
                : QuoteRequestStatus.PartiallyCompleted;

        UpdatedAtUtc = DateTime.UtcNow;
    }
}
