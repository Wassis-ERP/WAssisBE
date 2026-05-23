using System.Text.Json;

namespace WAssis.Services.Api.Modules.Opportunities.Contracts;

public sealed record UpdateOpportunityRequest(
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
    JsonElement? Metadata);
