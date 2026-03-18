namespace Fargo.Domain.Entities;

/// <summary>
/// Represents a user's access to a specific partition.
/// </summary>
public interface IPartitionAccess
{
    /// <summary>
    /// Gets the partition associated with this access.
    /// </summary>
    IPartition Partition { get; }
}