namespace Fargo.Core.Entities.Abstracts;

/// <summary>
/// Base class for all entities in the system.
/// Each entity is uniquely identified by a GUID.
/// </summary>
public abstract class Entity
{
    public Guid Guid { get; }

    public Entity()
    {
        Guid = Guid.NewGuid();
    }

    public Entity(Guid guid)
    {
        Guid = guid;
    }
}
