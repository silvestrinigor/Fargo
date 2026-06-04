namespace Fargo.Core.Activables;

/// <summary>
/// Provides extension operations for activable entities.
/// </summary>
public static class ValidateActiveExtension
{
    extension<TEntity>(TEntity entity)
        where TEntity : IActivable
    {
        /// <summary>
        /// Validates that the entity is active.
        /// </summary>
        /// <exception cref="EntityNotActiveException{TEntity}">
        /// Thrown when the entity is inactive.
        /// </exception>
        public void ValidateIsActive()
        {
            if (!entity.IsActive)
            {
                throw new EntityNotActiveException<TEntity>(entity);
            }
        }
    }
}
