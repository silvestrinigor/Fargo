namespace Fargo.Sdk;

public sealed class EntityDeletedEventArgs
{
    internal EntityDeletedEventArgs(Guid entityGuid)
    {
        EntityGuid = entityGuid;
    }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }
}
