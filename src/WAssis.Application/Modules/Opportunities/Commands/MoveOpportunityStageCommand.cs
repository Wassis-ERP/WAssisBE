using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Modules.Opportunities.Dtos;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed record MoveOpportunityStageCommand(Guid Id, string? StageId) : ICommand<OpportunityDto?>;
