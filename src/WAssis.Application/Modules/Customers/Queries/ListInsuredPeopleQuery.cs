using MediatR;
using WAssis.Application.Modules.Customers.Dtos;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed record ListInsuredPeopleQuery(string? Search, string? Status) : IRequest<IReadOnlyCollection<InsuredPersonDto>>;
