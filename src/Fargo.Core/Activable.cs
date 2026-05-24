namespace Fargo.Core;

/// <summary>
/// Represents an entity that can be activated or deactivated.
/// </summary>
/// <remarks>
/// Activable entities can participate in business rules that require the entity
/// to be active before an operation is allowed.
/// </remarks>
public interface IActivableEntity : IEntity
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }
}

/// <summary>
/// Thrown when an operation requires an entity to be active,
/// but the entity is inactive.
/// </summary>
/// <typeparam name="TEntity">
/// The type of the inactive entity.
/// </typeparam>
public class EntityNotActiveFargoDomainException<TEntity> : FargoDomainException
    where TEntity : IActivableEntity
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

/// <summary>
/// Provides extension operations for activable entities.
/// </summary>
public static class ActivableExtensions
{
    extension<TEntity>(TEntity entity)
        where TEntity : IActivableEntity
    {
        /// <summary>
        /// Validates that the entity is active.
        /// </summary>
        /// <exception cref="EntityNotActiveFargoDomainException{TEntity}">
        /// Thrown when the entity is inactive.
        /// </exception>
        public void ValidateIsActive()
        {
            if (!entity.IsActive)
            {
                throw new EntityNotActiveFargoDomainException<TEntity>(entity);
            }
        }
    }
}
