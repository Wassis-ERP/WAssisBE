using System.Text.Json;

namespace WAssis.Services.Api.Modules.Opportunities.Contracts;

public sealed record CreateOpportunityRequest(
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
    JsonElement? Metadata);
