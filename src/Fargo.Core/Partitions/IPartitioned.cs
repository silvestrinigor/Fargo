using Fargo.Core.Entities;

namespace Fargo.Core.Partitions;

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitioned : IEntity
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<IPartition> Partitions { get; }

    void AddPartition(Partition partition);

    void RemovePartition(Partition partition);
}
