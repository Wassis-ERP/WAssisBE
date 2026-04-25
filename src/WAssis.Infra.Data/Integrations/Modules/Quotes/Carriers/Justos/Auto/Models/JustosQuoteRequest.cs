namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Models;

public sealed record JustosQuoteRequest(
    string Plate,
    string Cep,
    JustosQuoteUser User,
    string VehicleFipeCode,
    string VehicleModelYear,
    bool Under24,
    bool IsInsured,
    string PreviousBonus,
    int BrokerCommissionPercentage,
    int? InsurerCode);

public sealed record JustosQuoteUser(
    string Cpf,
    string Name,
    string Surname,
    string Gender,
    string BirthDate);
