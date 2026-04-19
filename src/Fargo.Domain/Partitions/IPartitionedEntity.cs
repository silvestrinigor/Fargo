namespace Fargo.Domain.Partitions;

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitionedEntity
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<IPartitionEntity> Partitions { get; }
}
