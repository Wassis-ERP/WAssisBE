using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed record CreateOpportunityCommand(
    string? OfficeBranchId,
    string Name,
    string? ResponsibleId,
    Guid? InsuredPersonId,
    string? PipelineId,
    string? StageId,
    string? InsuranceLineId,
    string? InsurerId,
    string? OriginId,
    string? Status,
    string? BusinessType,
    bool? ContactType,
    decimal? NetPremium,
    decimal? CommissionPercentage,
    decimal? AgencyPercentage,
    DateTime? ValidityStartUtc,
    DateTime? ValidityEndUtc,
    DateTime? NextFollowUpUtc,
    string? Referrer,
    string? Notes,
    string? MetadataJson) : IRequest<OpportunityDto>;
