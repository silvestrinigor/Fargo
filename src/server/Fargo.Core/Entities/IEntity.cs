namespace Fargo.Core.Entities;

/// <summary>
/// Represents an entity with a unique identifier.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Guid { get; }
}
