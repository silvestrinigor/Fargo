namespace Fargo.Sdk;

public sealed class EntityCreatedEventArgs
{
    internal EntityCreatedEventArgs(Guid entityGuid)
    {
        EntityGuid = entityGuid;
    }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }
}
