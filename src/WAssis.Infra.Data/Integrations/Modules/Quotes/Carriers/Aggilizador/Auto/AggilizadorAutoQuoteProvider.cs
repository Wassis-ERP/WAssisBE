using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto.Clients;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto;

public sealed class AggilizadorAutoQuoteProvider(
    IOptions<AggilizadorAutoQuoteOptions> optionsAccessor,
    AggilizadorAutoQuoteClient quoteClient)
    : IQuoteProvider
{
    private readonly AggilizadorAutoQuoteOptions _options = optionsAccessor.Value;

    public string ProviderCode => "aggilizador_auto";
    public string ProviderName => "Aggilizador Auto";
    public bool IsEnabled => _options.Enabled;

    public QuoteProviderDescriptorDto Describe()
    {
        var requirements = new[]
        {
            new QuoteProviderRequirementDto("base_url", "Configurar a BaseUrl da API do Aggilizador.", !string.IsNullOrWhiteSpace(_options.BaseUrl)),
            new QuoteProviderRequirementDto("insurance_broker_id", "Configurar o InsuranceBrokerId fornecido pela Agger.", _options.InsuranceBrokerId > 0),
            new QuoteProviderRequirementDto("partner_id", "Configurar o PartnerId fornecido para a parceria.", _options.PartnerId >= 0)
        };

        return new QuoteProviderDescriptorDto(
            ProviderCode,
            ProviderName,
            IsEnabled,
            requirements.All(static requirement => requirement.IsConfigured),
            "Partner Id + Broker Id",
            "auto",
            null,
            requirements,
            [
                new QuoteStatusMessageDto(
                    "aggilizador_contingency_provider",
                    "Provider contingencial de multicálculo pronto para gestao operacional e fallback do fluxo principal.")
            ]);
    }

    public async Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(
        StartQuoteProcessingDto request,
        CancellationToken cancellationToken)
    {
        var descriptor = Describe();
        var missingRequirements = descriptor.Requirements
            .Where(static requirement => !requirement.IsConfigured)
            .Select(static requirement => new QuoteStatusMessageDto(
                $"{requirement.Code}_missing",
                requirement.Description))
            .ToArray();

        if (missingRequirements.Length > 0)
        {
            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.LoginInvalid,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    descriptor.Messages.Concat(missingRequirements).ToArray())
            ];
        }

        var validationMessages = ValidateRequest(request);
        if (validationMessages.Count > 0)
        {
            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.Restriction,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    validationMessages)
            ];
        }

        var payload = BuildPayload(request);
        using var response = await quoteClient.CreateQuoteAsync(payload, cancellationToken);
        if (response is null)
        {
            return
            [
                CreateSingleResult(
                    QuoteOptionStatus.Failure,
                    $"{ProviderCode}-{request.QuoteRequestId:N}",
                    [new QuoteStatusMessageDto("aggilizador_unavailable", "O Aggilizador nao retornou uma resposta valida para a cotacao contingencial.")])
            ];
        }

        var root = response.RootElement;
        var externalReference = ExtractString(root, "Id", "id", "QuoteId", "quoteId", "CalculationId")
            ?? $"{ProviderCode}-{request.QuoteRequestId:N}";

        return
        [
            new QuoteProviderResultDto(
                ProviderCode,
                ProviderName,
                QuoteOptionStatus.Ok,
                externalReference,
                ExtractMoney(root, "Premium", "premium", "TotalPremium", "totalPremium", "ValorPremio"),
                null,
                [],
                [],
                [
                    new QuoteStatusMessageDto(
                        "aggilizador_quote_registered",
                        "Solicitacao registrada no Aggilizador Auto como contingencia operacional.")
                ])
        ];
    }

    private object BuildPayload(StartQuoteProcessingDto request)
    {
        var vigenciaInicial = DateTime.Today;
        var vigenciaFinal = vigenciaInicial.AddYears(1);
        var commissionPercentage = request.BrokerCommissionPercentage ?? _options.DefaultCommissionPercentage;
        var parsedPhone = SplitPhone(request.PhoneNumber);

        return new
        {
            BrokerId = (int?)null,
            DeviceId = (string?)null,
            InsuranceBroker = string.Empty,
            InsuranceBrokerId = _options.InsuranceBrokerId,
            Partner = _options.PartnerId,
            CalculationAuto = new
            {
                Segurado = new
                {
                    CpfCnpj = request.DocumentNumber,
                    NomeCompleto = request.CustomerName,
                    DataNascimento = request.CustomerBirthDateUtc?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Sexo = MapGender(request.CustomerGender),
                    EstadoCivil = request.CustomerMaritalStatusCode,
                    TempoHabilitacao = request.DriverLicenseYears?.ToString(CultureInfo.InvariantCulture),
                    NumeroHabilitacao = request.DriverLicenseNumber ?? string.Empty,
                    Cep = request.PostalCode,
                    Email = (string?)null,
                    TelefoneResidencial = new
                    {
                        Ddd = parsedPhone.ddd,
                        Numero = parsedPhone.number
                    },
                    TelefoneCelular = new
                    {
                        Ddd = parsedPhone.ddd,
                        Numero = parsedPhone.number
                    },
                    RelacaoSeguradoCondutor = request.InsuredDriverRelationshipCode,
                    Perfil = true
                },
                CondutorPrincipal = new
                {
                    CpfCnpj = request.DocumentNumber,
                    NomeCompleto = request.CustomerName,
                    DataNascimento = request.CustomerBirthDateUtc?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Sexo = MapGender(request.CustomerGender),
                    EstadoCivil = request.CustomerMaritalStatusCode,
                    TempoHabilitacao = request.DriverLicenseYears?.ToString(CultureInfo.InvariantCulture),
                    NumeroHabilitacao = request.DriverLicenseNumber ?? string.Empty
                },
                Veiculo = new
                {
                    NumeroChassi = request.VehicleChassisNumber ?? string.Empty,
                    Placa = request.VehiclePlate ?? string.Empty,
                    Modelo = request.VehicleModel ?? string.Empty,
                    Fabricante = request.VehicleBrand ?? string.Empty,
                    AnoFabricacao = (request.VehicleManufactureYear ?? request.VehicleModelYear).ToString(CultureInfo.InvariantCulture),
                    AnoModelo = request.VehicleModelYear.ToString(CultureInfo.InvariantCulture),
                    CodigoFipe = request.VehicleFipeCode ?? string.Empty,
                    ZeroKm = ToAggBoolean(request.VehicleIsZeroKm),
                    Rastreador = ToAggBoolean(request.VehicleHasTracker),
                    Antifurto = ToAggBoolean(request.VehicleHasAntiTheft),
                    Alienado = ToAggBoolean(request.VehicleIsFinanced),
                    Blindado = ToAggBoolean(request.VehicleIsArmored),
                    Combustivel = request.VehicleFuelTypeCode,
                    CepPernoite = request.VehicleOvernightPostalCode ?? request.PostalCode,
                    KitGas = ToAggBoolean(request.VehicleHasKitGas),
                    Fipe = new
                    {
                        Codigo = request.VehicleFipeCode ?? string.Empty,
                        Modelo = request.VehicleModel ?? string.Empty,
                        Marca = request.VehicleBrand ?? string.Empty,
                        AnoMaximo = request.VehicleModelYear.ToString(CultureInfo.InvariantCulture),
                        AnoMinimo = (request.VehicleManufactureYear ?? request.VehicleModelYear).ToString(CultureInfo.InvariantCulture),
                        TipoVeiculo = 0
                    }
                },
                Questionario = new
                {
                    TipoResidencia = _options.DefaultResidenceTypeCode,
                    VeiculosResidencia = _options.DefaultVehiclesAtResidenceCode,
                    QuilometragemMensal = _options.DefaultMonthlyMileageCode,
                    GaragemTrabalho = _options.DefaultWorkGarageCode,
                    GaragemResidencia = _options.DefaultResidenceGarageCode,
                    GaragemEstudo = _options.DefaultStudyGarageCode,
                    UsoVeiculo = _options.DefaultVehicleUsageCode,
                    UsoDependentes = _options.DefaultDependentVehicleUsageCode,
                    FaixaEtariaDependentes = _options.DefaultDependentAgeRangeCode,
                    DistanciaResidenciaTrabalho = _options.DefaultDistanceResidenceWorkCode,
                    Profissao = _options.DefaultProfessionCode,
                    Associado = string.Empty,
                    PeriodoUso = string.Empty,
                    IsencaoFiscal = (string?)null,
                    Pcd = _options.DefaultPcdCode
                },
                Caminhao = (object?)null,
                Cobertura = new
                {
                    DanosMorais = _options.DefaultMoralDamagesAmount,
                    DanosMateriais = _options.DefaultMaterialDamagesAmount,
                    DanosCorporais = _options.DefaultBodilyDamagesAmount,
                    MorteInvalidez = _options.DefaultDeathInvalidityAmount,
                    TipoFranquia = _options.DefaultDeductibleTypeCode,
                    Assistencia = _options.DefaultAssistanceCode,
                    Vidros = _options.DefaultGlassCoverageCode,
                    CarroReserva = _options.DefaultReserveCarCode,
                    FatorAjuste = _options.DefaultAdjustmentFactor,
                    KitGas = request.VehicleHasKitGas == true ? "1.00" : "0.00",
                    Carroceria = "0.00",
                    Equipamento = "0.00",
                    ArCondicionado = "0"
                },
                Seguro = new
                {
                    Banco = _options.DefaultBankCode,
                    Bonus = request.PreviousBonus ?? string.Empty,
                    TipoSeguro = _options.DefaultInsuranceTypeCode,
                    Comissao = commissionPercentage.ToString(CultureInfo.InvariantCulture),
                    VigenciaInicial = vigenciaInicial.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture),
                    VigenciaFinal = vigenciaFinal.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture),
                    VigenciaFinalAnterior = (string?)null,
                    SeguradoraAnterior = request.RenewalInsurerCode?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    CodigoIdentificacao = string.Empty,
                    NumeroApoliceAnterior = string.Empty,
                    Sinistros = _options.DefaultSinisterCode,
                    Observacoes = string.Empty
                }
            },
            Renovation = request.IsCurrentlyInsured
        };
    }

    private static List<QuoteStatusMessageDto> ValidateRequest(StartQuoteProcessingDto request)
    {
        var messages = new List<QuoteStatusMessageDto>();

        AddMissing(messages, request.PostalCode, "missing_postal_code", "O Aggilizador exige CEP do segurado.");
        AddMissing(messages, request.CustomerGender, "missing_gender", "O Aggilizador exige o sexo do segurado.");
        AddMissing(messages, request.CustomerMaritalStatusCode, "missing_marital_status", "O Aggilizador exige o estado civil do segurado.");
        AddMissing(messages, request.InsuredDriverRelationshipCode, "missing_driver_relationship", "O Aggilizador exige a relacao entre segurado e condutor.");
        AddMissing(messages, request.VehiclePlate, "missing_plate", "O Aggilizador exige a placa do veiculo.");
        AddMissing(messages, request.VehicleBrand, "missing_vehicle_brand", "O Aggilizador exige a marca do veiculo.");
        AddMissing(messages, request.VehicleModel, "missing_vehicle_model", "O Aggilizador exige o modelo FIPE do veiculo.");
        AddMissing(messages, request.VehicleFipeCode, "missing_fipe_code", "O Aggilizador exige o codigo FIPE.");
        AddMissing(messages, request.VehicleFuelTypeCode, "missing_fuel_type", "O Aggilizador exige o tipo de combustivel.");

        if (request.CustomerBirthDateUtc is null)
        {
            messages.Add(new QuoteStatusMessageDto("missing_birth_date", "O Aggilizador exige a data de nascimento do segurado."));
        }

        if (!request.DriverLicenseYears.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_driver_license_years", "O Aggilizador exige o tempo de habilitacao."));
        }

        if (!request.VehicleManufactureYear.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_manufacture_year", "O Aggilizador exige o ano de fabricacao do veiculo."));
        }

        if (!request.VehicleIsZeroKm.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_zero_km", "O Aggilizador exige informacao se o veiculo e zero km."));
        }

        if (!request.VehicleHasTracker.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_tracker", "O Aggilizador exige informacao sobre rastreador."));
        }

        if (!request.VehicleHasAntiTheft.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_anti_theft", "O Aggilizador exige informacao sobre antifurto."));
        }

        if (!request.VehicleIsFinanced.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_financed_flag", "O Aggilizador exige informacao se o veiculo e alienado."));
        }

        if (!request.VehicleIsArmored.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_armored_flag", "O Aggilizador exige informacao se o veiculo e blindado."));
        }

        if (!request.VehicleHasKitGas.HasValue)
        {
            messages.Add(new QuoteStatusMessageDto("missing_kit_gas_flag", "O Aggilizador exige informacao sobre kit gas."));
        }

        return messages;
    }

    private static void AddMissing(
        ICollection<QuoteStatusMessageDto> messages,
        string? value,
        string code,
        string description)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            messages.Add(new QuoteStatusMessageDto(code, description));
        }
    }

    private static string ToAggBoolean(bool? value) => value == true ? "1" : "0";

    private static string? MapGender(string? gender)
    {
        return gender?.Trim().ToUpperInvariant() switch
        {
            "M" => "1",
            "F" => "2",
            _ => null
        };
    }

    private static (string? ddd, string? number) SplitPhone(string? source)
    {
        var digits = new string((source ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length >= 10)
        {
            return ($"({digits[..2]})", digits[2..Math.Min(digits.Length, 11)]);
        }

        return (null, null);
    }

    private static decimal? ExtractMoney(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
            {
                return value;
            }

            if (property.ValueKind == JsonValueKind.String &&
                decimal.TryParse(
                    property.GetString()?.Replace(".", "").Replace(",", "."),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static string? ExtractString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                continue;
            }

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.ToString(),
                _ => null
            };
        }

        return null;
    }

    private QuoteProviderResultDto CreateSingleResult(
        QuoteOptionStatus status,
        string externalReference,
        IReadOnlyCollection<QuoteStatusMessageDto> messages)
    {
        return new QuoteProviderResultDto(
            ProviderCode,
            ProviderName,
            status,
            externalReference,
            null,
            null,
            [],
            [],
            messages);
    }
}
