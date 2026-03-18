namespace Fargo.Domain.Entities;

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitioned
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<Partition> Partitions { get; }
}
