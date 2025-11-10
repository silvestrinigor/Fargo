namespace Fargo.Core.Entities.Abstracts;

/// <summary>
/// Base class for all entities in the system.
/// Each entity is uniquely identified by a GUID.
/// </summary>
public abstract class BaseEntity
{
    public Guid Guid { get; }

    public BaseEntity()
    {
        Guid = Guid.NewGuid();
    }

    public BaseEntity(Guid guid)
    {
        Guid = guid;
    }
}
