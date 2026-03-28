using System.Text.Json.Serialization;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Models;

public sealed record JustosQuoteRequest(
    [property: JsonPropertyName("plate")] string Plate,
    [property: JsonPropertyName("cep")] string Cep,
    [property: JsonPropertyName("user")] JustosQuoteUser User,
    [property: JsonPropertyName("vehicle_fipe_code")] string VehicleFipeCode,
    [property: JsonPropertyName("vehicle_model_year")] string VehicleModelYear,
    [property: JsonPropertyName("under_24")] bool Under24,
    [property: JsonPropertyName("is_insured")] bool IsInsured,
    [property: JsonPropertyName("previous_bonus")] string PreviousBonus,
    [property: JsonPropertyName("broker_commission_percentage")] int BrokerCommissionPercentage,
    [property: JsonPropertyName("insurer_code")] int? InsurerCode);

public sealed record JustosQuoteUser(
    [property: JsonPropertyName("cpf")] string Cpf,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("birth_date")] string BirthDate);
