namespace WAssis.Services.Api.Modules.Policies.Contracts;

public sealed record UpdatePolicyDraftStatusRequest(string? Notes, string? PolicyNumber = null);
