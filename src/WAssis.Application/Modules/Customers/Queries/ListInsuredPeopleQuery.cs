using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed record ListInsuredPeopleQuery(string? Search, string? Status, int Page, int PageSize) : IQuery<PagedResult<InsuredPersonDto>>;
