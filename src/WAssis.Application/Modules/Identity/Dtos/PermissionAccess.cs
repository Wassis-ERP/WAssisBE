namespace WAssis.Application.Modules.Identity.Dtos;

public sealed record PermissionAccess(bool Allowed, bool OwnRecordsOnly, string[] ProducerIds)
{
    public static PermissionAccess Denied { get; } = new(false, false, []);
    public static PermissionAccess Branch { get; } = new(true, false, []);
}
