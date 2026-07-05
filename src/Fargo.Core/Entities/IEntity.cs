namespace Fargo.Core.Entities;

/// <summary>
/// Defines the contract for domain entities identified by a <see cref="Guid"/>.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Guid { get; }
}
