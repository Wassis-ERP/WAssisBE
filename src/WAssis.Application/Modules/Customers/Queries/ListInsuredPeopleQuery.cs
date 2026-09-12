using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Customers.Dtos;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed record ListInsuredPeopleQuery(string? Search, string? Status) : IQuery<IReadOnlyCollection<InsuredPersonDto>>;
