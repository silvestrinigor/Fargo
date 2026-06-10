namespace Fargo.Core.Activables;

public class EntityActiveException<TEntity> : Exception
    where TEntity : IActivable
{
    public EntityActiveException(TEntity entity)
        : base($"{typeof(TEntity).Name} {entity.Guid} is active.")
    {
        EntityGuid = entity.Guid;
    }

    public EntityActiveException(TEntity entity, string message)
        : base(message)
    {
        EntityGuid = entity.Guid;
    }

    public Guid EntityGuid { get; }
}
