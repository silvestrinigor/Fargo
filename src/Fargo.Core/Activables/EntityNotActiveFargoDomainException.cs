namespace Fargo.Core.Activables;

/// <summary>
/// Thrown when an operation requires an entity to be active,
/// but the entity is inactive.
/// </summary>
/// <typeparam name="TEntity">
/// The type of the inactive entity.
/// </typeparam>
public class EntityNotActiveFargoDomainException<TEntity> : FargoDomainException
    where TEntity : IActivable
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EntityNotActiveFargoDomainException{TEntity}"/> class.
    /// </summary>
    /// <param name="entity">
    /// The inactive entity that caused the exception.
    /// </param>
    public EntityNotActiveFargoDomainException(TEntity entity)
        : base($"{typeof(TEntity).Name} {entity.Guid} is not active.")
    {
        EntityGuid = entity.Guid;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EntityNotActiveFargoDomainException{TEntity}"/> class
    /// with a custom error message.
    /// </summary>
    /// <param name="entity">
    /// The inactive entity that caused the exception.
    /// </param>
    /// <param name="message">
    /// The custom exception message.
    /// </param>
    public EntityNotActiveFargoDomainException(TEntity entity, string message)
        : base(message)
    {
        EntityGuid = entity.Guid;
    }

    /// <summary>
    /// Gets the identifier of the inactive entity.
    /// </summary>
    public Guid EntityGuid { get; }
}
