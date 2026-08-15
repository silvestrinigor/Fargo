namespace Fargo.Core.Entities;

/// <summary>
/// Represents an entity that exposes its corresponding <see cref="EntityType"/>.
/// </summary>
public interface IEntityTyped
{
    /// <summary>
    /// Gets the type of the entity.
    /// </summary>
    /// <returns>
    /// The <see cref="EntityType"/> that identifies the entity type.
    /// </returns>
    EntityType GetEntityType();
}
