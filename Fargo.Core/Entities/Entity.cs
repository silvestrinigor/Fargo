namespace Fargo.Core.Entities;

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
