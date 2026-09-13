namespace WAssis.Infra.CrossCutting.Identity.Authentication;

/// <summary>Temporary, explicitly enabled Staging authentication. Never valid in Production.</summary>
public sealed class HomologationAuthOptions
{
    public const string SectionName = "Identity:HomologationAuth";
    public bool Enabled { get; set; }
    public int TokenExpirationMinutes { get; set; } = 15;
    public List<HomologationAuthUserOptions> Users { get; set; } = [];
}

public sealed class HomologationAuthUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BrokerageId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public List<string> BranchIds { get; set; } = [];
    public bool HasAllBranchesAccess { get; set; }
    public string? SellerId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
