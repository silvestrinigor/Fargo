namespace Fargo.Sdk.Entities;

public abstract class Entity
{
    internal Entity()
    {

    }

    public Guid Guid { get; init; }

    public bool IsDeleted { get; private set; }

    internal void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}
