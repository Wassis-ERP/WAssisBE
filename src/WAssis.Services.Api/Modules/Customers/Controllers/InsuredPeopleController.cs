using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Customers.Commands;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Customers.Contracts;
using WAssis.Services.Api.Modules.Customers.ViewModels;

namespace WAssis.Services.Api.Modules.Customers.Controllers;

[ApiController]
[Route("api/segurados")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class InsuredPeopleController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<InsuredPersonViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new ListInsuredPeopleQuery(search, status), cancellationToken);
        return Ok(response.Select(ToViewModel).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InsuredPersonViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetInsuredPersonQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    [HttpPost]
    [ProducesResponseType(typeof(InsuredPersonViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateInsuredPersonRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateInsuredPersonCommand(
            request.OfficeBranchId,
            request.Name,
            request.PersonType,
            request.Status,
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.BirthDateUtc,
            request.TradeName,
            request.Gender,
            request.MaritalStatus,
            request.CompanySize,
            request.Cnae,
            request.Website,
            request.PostalCode,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            request.Notes,
            request.ProducerId,
            request.ManagerId,
            request.ChatwootId,
            request.LgpdAuthorized ?? false,
            request.SocialName,
            request.IdentityDocument,
            request.MunicipalRegistration,
            request.EconomicActivity,
            request.Profession,
            request.MonthlyIncome,
            request.DriverLicenseNumber,
            request.DriverLicenseCategory,
            request.DriverLicenseExpirationDate,
            request.MobilePhoneNumber,
            request.SecondaryPhoneNumber,
            request.WhatsAppNumber,
            request.Country,
            request.LgpdAuthorizedAtUtc,
            request.ImportOrigin), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ToViewModel(response));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InsuredPersonViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInsuredPersonRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new UpdateInsuredPersonCommand(
            id,
            request.OfficeBranchId,
            request.Name,
            request.PersonType,
            request.Status,
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.BirthDateUtc,
            request.TradeName,
            request.Gender,
            request.MaritalStatus,
            request.CompanySize,
            request.Cnae,
            request.Website,
            request.PostalCode,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            request.Notes,
            request.ProducerId,
            request.ManagerId,
            request.ChatwootId,
            request.LgpdAuthorized ?? false,
            request.SocialName,
            request.IdentityDocument,
            request.MunicipalRegistration,
            request.EconomicActivity,
            request.Profession,
            request.MonthlyIncome,
            request.DriverLicenseNumber,
            request.DriverLicenseCategory,
            request.DriverLicenseExpirationDate,
            request.MobilePhoneNumber,
            request.SecondaryPhoneNumber,
            request.WhatsAppNumber,
            request.Country,
            request.LgpdAuthorizedAtUtc,
            request.ImportOrigin), cancellationToken);

        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    private static InsuredPersonViewModel ToViewModel(InsuredPersonDto response)
    {
        return new InsuredPersonViewModel(
            response.Id,
            response.OfficeBranchId,
            response.Name,
            response.PersonType,
            response.Status,
            response.DocumentNumber,
            response.Email,
            response.PhoneNumber,
            response.BirthDateUtc,
            response.TradeName,
            response.Gender,
            response.MaritalStatus,
            response.CompanySize,
            response.Cnae,
            response.Website,
            response.PostalCode,
            response.Street,
            response.Number,
            response.Complement,
            response.Neighborhood,
            response.City,
            response.State,
            response.Notes,
            response.ProducerId,
            response.ManagerId,
            response.ChatwootId,
            response.LgpdAuthorized,
            response.CreatedBy,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.SocialName,
            response.IdentityDocument,
            response.MunicipalRegistration,
            response.EconomicActivity,
            response.Profession,
            response.MonthlyIncome,
            response.DriverLicenseNumber,
            response.DriverLicenseCategory,
            response.DriverLicenseExpirationDate,
            response.MobilePhoneNumber,
            response.SecondaryPhoneNumber,
            response.WhatsAppNumber,
            response.Country,
            response.LgpdAuthorizedAtUtc,
            response.ImportOrigin);
    }
}
