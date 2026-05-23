using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed record UpdateOpportunityCommand(
    Guid Id,
    string? OfficeBranchId,
    string Name,
    string? ResponsibleId,
    Guid? InsuredPersonId,
    string? PipelineId,
    string? StageId,
    string? InsuranceLineId,
    string? InsurerId,
    string? OriginId,
    string? LossReasonId,
    string? Status,
    string? BusinessType,
    bool? ContactType,
    decimal? NetPremium,
    decimal? CommissionPercentage,
    decimal? AgencyPercentage,
    decimal? ProductionAmount,
    DateTime? ValidityStartUtc,
    DateTime? ValidityEndUtc,
    DateTime? NextFollowUpUtc,
    DateTime? ConcludedAtUtc,
    string? Referrer,
    string? Notes,
    string? MetadataJson) : IRequest<OpportunityDto?>;
