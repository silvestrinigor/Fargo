namespace Fargo.Domain.Entities;

// TODO: validate this documentation
/// <summary>
/// Defines the contract for domain entities identified by a <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// An entity is uniquely identified by its <see cref="Guid"/> and uses
/// identity-based equality semantics.
///
/// Two entities are considered equal when they are of the same concrete type
/// and have the same non-empty identifier.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <remarks>
    /// The identifier uniquely distinguishes the entity within the domain.
    /// It must not be <see cref="Guid.Empty"/>.
    /// </remarks>
    Guid Guid { get; }
}
