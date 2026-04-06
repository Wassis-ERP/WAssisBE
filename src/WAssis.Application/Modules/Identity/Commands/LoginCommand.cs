using MediatR;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Identity.Commands;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResultDto>>;
