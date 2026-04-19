namespace Fargo.Domain.Partitions;

/// <summary>
/// Represents an entity that has access to one or more partitions.
/// </summary>
public interface IPartitionUser
{
    /// <summary>
    /// Gets the collection of partition accesses granted to the user.
    /// </summary>
    IReadOnlyCollection<IPartitionAccess> PartitionAccesses { get; }
}
