namespace WAssis.Application.Abstractions;

// Request-scoped and assigned only by server authorization. Never bind this
// object from request data or use it to replace tenant/branch filters.
public sealed class RecordPermissionScope
{
    public string? Module { get; private set; }
    public string? UserId { get; private set; }
    public string[] ProducerIds { get; private set; } = [];
    public bool OwnRecordsOnly { get; private set; }

    public void RestrictToOwner(string module, string userId, string[] producerIds)
    {
        Module = module;
        UserId = userId;
        ProducerIds = producerIds;
        OwnRecordsOnly = true;
    }
}
