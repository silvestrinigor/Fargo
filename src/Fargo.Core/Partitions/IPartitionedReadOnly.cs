namespace Fargo.Core.Partitions;

/// <summary>
/// Represents an entity that is associated with partitions.
/// </summary>
public interface IPartitionedReadOnly
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<Partition> Partitions { get; }
}
