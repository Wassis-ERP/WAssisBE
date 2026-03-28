using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Domain.Modules.Quotes.Entities;

public class QuoteRequest : AggregateRoot
{
    private readonly List<QuoteOption> _options = [];

    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? VehiclePlate { get; private set; }
    public string? VehicleBrand { get; private set; }
    public string? VehicleModel { get; private set; }
    public int VehicleModelYear { get; private set; }
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
        string correlationId,
        string customerName,
        string documentNumber,
        string? email,
        string? phoneNumber,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        int vehicleModelYear)
    {
        Id = id;
        CorrelationId = correlationId;
        CustomerName = customerName;
        DocumentNumber = documentNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        VehiclePlate = vehiclePlate;
        VehicleBrand = vehicleBrand;
        VehicleModel = vehicleModel;
        VehicleModelYear = vehicleModelYear;
        Status = QuoteRequestStatus.Pending;
        ShareToken = Guid.NewGuid().ToString("N");
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static QuoteRequest Create(
        string correlationId,
        string customerName,
        string documentNumber,
        string? email,
        string? phoneNumber,
        string? vehiclePlate,
        string? vehicleBrand,
        string? vehicleModel,
        int vehicleModelYear)
    {
        return new QuoteRequest(
            Guid.NewGuid(),
            correlationId,
            customerName,
            documentNumber,
            email,
            phoneNumber,
            vehiclePlate,
            vehicleBrand,
            vehicleModel,
            vehicleModelYear);
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
