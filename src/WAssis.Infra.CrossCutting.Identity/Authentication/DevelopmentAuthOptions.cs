namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class DevelopmentAuthOptions
{
    public const string SectionName = "Identity:DevelopmentAuth";

    public bool Enabled { get; set; } = true;
    public bool AllowOutsideDevelopment { get; set; }
    public int TokenExpirationMinutes { get; set; } = 480;
    public List<DevelopmentAuthUserOptions> Users { get; set; } = [];
}

public sealed class DevelopmentAuthUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? BrokerageId { get; set; }
    public string? BranchId { get; set; }
    public List<string> BranchIds { get; set; } = [];
    public bool HasAllBranchesAccess { get; set; }
    public string? SellerId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
