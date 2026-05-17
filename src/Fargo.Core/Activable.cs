namespace Fargo.Core;

/// <summary>
/// Represents an entity that can be activated or deactivated.
/// </summary>
public interface IActivableEntity : IEntity
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Activates the entity.
    /// </summary>
    void Activate();

    /// <summary>
    /// Deactivates the entity.
    /// </summary>
    void Deactivate();
}

/// <summary>
/// Thrown when an operation requires an entity to be active,
/// but the entity is inactive.
/// </summary>
public class EntityNotActiveFargoDomainException<TEntity> : FargoDomainException
    where TEntity : IEntity, IActivableEntity
{
    public EntityNotActiveFargoDomainException(TEntity entity) : base($"{typeof(TEntity).Name} {entity.Guid} is not active.")
    {
        EntityGuid = entity.Guid;
    }

    public EntityNotActiveFargoDomainException(TEntity entity, string message) : base(message)
    {
        EntityGuid = entity.Guid;
    }

    /// <summary>
    /// Gets the identifier of the inactive entity.
    /// </summary>
    public Guid EntityGuid { get; }
}
