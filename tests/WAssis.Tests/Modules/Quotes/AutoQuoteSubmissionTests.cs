using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Validators;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Domain.Modules.Quotes.ValueObjects;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto.Clients;

namespace WAssis.Tests.Modules.Quotes;

public sealed class AutoQuoteSubmissionTests
{
    internal static AutoQuoteSubmission Submission(decimal limit = 100000) => new()
    {
        DriverName = "Condutora da versão", DriverDocument = "00000000001",
        DriverBirthDate = new(1993, 3, 12), DriverGender = "F", DriverMaritalStatus = "casado",
        DriverLicenseYears = 10, InsuranceType = "RENOVACAO_OUTRA",
        CoverageStartsOn = new(2028, 2, 29), CoverageEndsOn = new(2029, 2, 28),
        PreviousCoverageEndsOn = new(2028, 2, 29), PreviousPolicyNumber = "FIXTURE-ANTERIOR",
        BonusClass = 0, ClaimCount = 0,
        Coverages = [new() { Code = "casco", Selected = true, RequestedFipePercentage = 100, RequestedDeductibleType = "normal" },
            new() { Code = "danos-materiais", Selected = true, RequestedLimit = limit },
            new() { Code = "danos-morais", Selected = true, RequestedLimit = 0 },
            new() { Code = "vidros", Selected = false }]
    };

    private static StartQuoteProcessingDto Request(AutoQuoteSubmission? submission) => new(
        Guid.NewGuid(), "fixture", "Segurado da oportunidade", "00000000000", "11999999999", "01000000",
        null, "M", "1", new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc), 20, "HABILITACAO-SEGURADO", "1",
        "CHASSI-FICTICIO", "AAA0A00", "Marca", "Modelo", "000000-0", 2025, 2026,
        false, false, false, false, false, "1", "01000000", false, false, true, "0", 15, 1, submission);

    private static AggilizadorAutoQuoteProvider Provider(CaptureHandler handler, bool mappings = true)
    {
        var options = new AggilizadorAutoQuoteOptions
        {
            BaseUrl = "https://provider.invalid/", InsuranceBrokerId = 1, PartnerId = 1,
            InsuranceTypeCodes = mappings ? new() { ["RENOVACAO_OUTRA"] = "fixture-renewal" } : [],
            DriverMaritalStatusCodes = mappings ? new() { ["casado"] = "fixture-married" } : [],
            DeductibleTypeCodes = mappings ? new() { ["normal"] = "fixture-normal" } : [],
            // Intentionally different: these defaults must never overwrite requested limits.
            DefaultMaterialDamagesAmount = "999999.00", DefaultGlassCoverageCode = "999"
        };
        return new(Options.Create(options), new(new HttpClient(handler) { BaseAddress = new(options.BaseUrl) }));
    }

    [Fact]
    public async Task DifferentVersions_PreserveDriverDatesLimitsAndZero_WithoutClaimingCompletion()
    {
        using var handler = new CaptureHandler();
        var provider = Provider(handler);
        foreach (var amount in new[] { 100000m, 200000m })
        {
            var result = Assert.Single(await provider.StartQuoteAsync(Request(Submission(amount)), default));
            Assert.Equal(QuoteOptionStatus.Pending, result.Status);
            Assert.Null(result.PremiumAmount);
            Assert.Equal("", result.InsuranceCompanyCode);
            using var body = JsonDocument.Parse(handler.Bodies.Last());
            var auto = body.RootElement.GetProperty("CalculationAuto");
            Assert.Equal("00000000001", auto.GetProperty("CondutorPrincipal").GetProperty("CpfCnpj").GetString());
            Assert.Equal("Condutora da versão", auto.GetProperty("CondutorPrincipal").GetProperty("NomeCompleto").GetString());
            Assert.Equal("", auto.GetProperty("CondutorPrincipal").GetProperty("NumeroHabilitacao").GetString());
            Assert.Equal("00000000000", auto.GetProperty("Segurado").GetProperty("CpfCnpj").GetString());
            Assert.Equal(amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), auto.GetProperty("Cobertura").GetProperty("DanosMateriais").GetString());
            Assert.Equal("0.00", auto.GetProperty("Cobertura").GetProperty("DanosMorais").GetString());
            Assert.Equal("0", auto.GetProperty("Cobertura").GetProperty("Vidros").GetString());
            Assert.Equal("29/02/2028 00:00:00", auto.GetProperty("Seguro").GetProperty("VigenciaInicial").GetString());
            Assert.Equal("28/02/2029 00:00:00", auto.GetProperty("Seguro").GetProperty("VigenciaFinal").GetString());
            Assert.Equal("0", auto.GetProperty("Seguro").GetProperty("Bonus").GetString());
            Assert.Equal("FIXTURE-ANTERIOR", auto.GetProperty("Seguro").GetProperty("NumeroApoliceAnterior").GetString());
        }
        Assert.Equal(2, handler.Bodies.Count);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("unmapped")]
    [InlineData("coverage")]
    [InlineData("quantity")]
    [InlineData("own-renewal")]
    public async Task IncompleteOrUnsupportedRequest_DoesNotSendHttp(string scenario)
    {
        using var handler = new CaptureHandler();
        var submission = scenario switch
        {
            "missing" => null,
            "coverage" => Submission() with { Coverages = [.. Submission().Coverages, new() { Code = "desconhecida", Selected = true }] },
            "quantity" => Submission() with { Coverages = [.. Submission().Coverages, new() { Code = "carro-reserva", Selected = true, RequestedQuantity = 7 }] },
            "own-renewal" => Submission() with { InsuranceType = "RENOVACAO_PROPRIA" },
            _ => Submission()
        };
        var result = Assert.Single(await Provider(handler, scenario != "unmapped").StartQuoteAsync(Request(submission), default));
        Assert.Equal(QuoteOptionStatus.Restriction, result.Status);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    public void Validation_RejectsInvalidDatesDuplicateCoveragesAndBonus()
    {
        var validator = new AutoQuoteSubmissionValidator();
        Assert.True(validator.Validate(Submission()).IsValid);
        Assert.False(validator.Validate(Submission() with { BonusClass = 11 }).IsValid);
        Assert.False(validator.Validate(Submission() with { CoverageEndsOn = Submission().CoverageStartsOn }).IsValid);
        Assert.False(validator.Validate(Submission() with { InsuranceType = "NOVO" }).IsValid);
        Assert.False(validator.Validate(Submission() with { Coverages = [Submission().Coverages[0], Submission().Coverages[0]] }).IsValid);
        Assert.False(validator.Validate(Submission() with { Coverages = null! }).IsValid);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"Id\":\"fixture-accepted\",\"Premium\":1234}", Encoding.UTF8, "application/json") };
        }
    }
}
