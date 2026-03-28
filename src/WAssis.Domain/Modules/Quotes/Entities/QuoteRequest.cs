using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Domain.Modules.Quotes.Entities;

public class QuoteRequest : AggregateRoot
{
    private readonly List<QuoteOption> _options = [];

    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CustomerSurname { get; private set; }
    public string? CustomerGender { get; private set; }
    public DateTime? CustomerBirthDateUtc { get; private set; }
    public string? VehiclePlate { get; private set; }
    public string? VehicleBrand { get; private set; }
    public string? VehicleModel { get; private set; }
    public string? VehicleFipeCode { get; private set; }
    public int VehicleModelYear { get; private set; }
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
        DateTime? customerBirthDateUtc,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        string? vehicleFipeCode,
        int vehicleModelYear,
        bool hasDriverUnder24,
        bool isCurrentlyInsured,
        string? previousBonus,
        int? brokerCommissionPercentage,
        int? renewalInsurerCode)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        CustomerName = customerName;
        DocumentNumber = documentNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        PostalCode = postalCode;
        CustomerSurname = customerSurname;
        CustomerGender = customerGender;
        CustomerBirthDateUtc = customerBirthDateUtc;
        VehiclePlate = vehiclePlate;
        VehicleBrand = vehicleBrand;
        VehicleModel = vehicleModel;
        VehicleFipeCode = vehicleFipeCode;
        VehicleModelYear = vehicleModelYear;
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
        DateTime? customerBirthDateUtc,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        string? vehicleFipeCode,
        int vehicleModelYear,
        bool hasDriverUnder24,
        bool isCurrentlyInsured,
        string? previousBonus,
        int? brokerCommissionPercentage,
        int? renewalInsurerCode)
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
            customerBirthDateUtc,
            vehiclePlate,
            vehicleBrand,
            vehicleModel,
            vehicleFipeCode,
            vehicleModelYear,
            hasDriverUnder24,
            isCurrentlyInsured,
            previousBonus,
            brokerCommissionPercentage,
            renewalInsurerCode);
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
