using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Customers.Dtos;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed record GetInsuredPersonQuery(Guid Id) : IQuery<InsuredPersonDto?>;
