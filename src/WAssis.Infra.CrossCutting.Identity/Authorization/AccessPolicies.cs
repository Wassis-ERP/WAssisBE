namespace WAssis.Infra.CrossCutting.Identity.Authorization;

public static class AccessPolicies
{
    public const string AuthenticatedUser = "authenticated_user";
    public const string PlatformAdmin = "platform_admin";
    public const string BrokerageStaff = "brokerage_staff";
    public const string BrokerageAdmin = "brokerage_admin";
    public const string DigitalCustomer = "digital_customer";
}
