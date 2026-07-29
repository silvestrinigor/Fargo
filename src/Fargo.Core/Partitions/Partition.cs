using Fargo.Core.Entities;
using Fargo.Core.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Partitions;

/// <summary>
/// Represents a partition used to isolate and scope access to core entities.
/// </summary>
/// <remarks>
/// Partitions define hierarchical access boundaries in the system.
///
/// A partition may reference a parent partition, forming a hierarchy.
/// Access inheritance flows from parent to child, but not from child to parent.
///
/// This means that a user with access to a parent partition can also access
/// entities belonging to its descendant partitions. However, a user with access
/// only to a child partition cannot access entities belonging to its parent
/// partition or to other branches of the hierarchy.
///
/// The system contains a unique global partition at the top of the hierarchy.
/// The global partition has access to all entities contained in its descendant
/// partitions. Access to this partition is restricted to highly privileged users.
/// </remarks>
public class Partition : Entity
{
    /// <summary>
    /// Gets or sets the name of the partition.
    /// </summary>
    public required Name Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the partition.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the value indicating whether the partition is the global partition.
    /// </summary>
    public bool IsGlobalPartition => Guid == FargoCoreGuids.GlobalPartitionGuid;

    /// <summary>
    /// Gets the unique identifier of the parent partition, if any.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the current partition
    /// is a root partition in the hierarchy.
    /// </remarks>
    public Guid? ParentPartitionGuid { get; private set; }

    /// <summary>
    /// Gets the parent partition of the current partition, if any.
    /// </summary>
    /// <remarks>
    /// The parent partition defines the hierarchical relationship between partitions,
    /// enabling access inheritance from parent to child.
    /// </remarks>
    public Partition? ParentPartition { get; private set; }

    [MemberNotNullWhen(true, nameof(ParentPartitionGuid))]
    public bool HasParentPartition => ParentPartitionGuid is not null;

    private Partition()
    {
    }

    /// <summary>
    /// Creates a new partition.
    /// </summary>
    /// <param name="name">The name of the partition.</param>
    /// <param name="parentPartition">The parent partition of the partition.</param>
    /// <returns></returns>
    public static Partition CreatePartition(Name name, Partition parentPartition)
    {
        var partition = new Partition
        {
            Name = name
        };

        partition.SetParentPartition(parentPartition);

        return partition;
    }

    /// <summary>
    /// Creates a new global partition.
    /// </summary>
    /// <param name="name">The name of the global partition.</param>
    /// <returns></returns>
    public static Partition CreateGlobalPartition(Name name)
    {
        var globalPartition = new Partition
        {
            Guid = FargoCoreGuids.GlobalPartitionGuid,
            Name = name
        };

        return globalPartition;
    }

    public void SetParentPartition(Partition parentPartition)
    {
        if (IsGlobalPartition)
        {
            throw new FargoCoreException(
                "Global partition cannot be part of another partition.",
                FargoCoreErrorType.GlobalPartitionCannotBePartOfAnotherPartition);
        }

        if (parentPartition.Guid == Guid)
        {
            throw new PartitionCannotBeOwnParentFargoCoreException(Guid);
        }

        ParentPartition = parentPartition;

        ParentPartitionGuid = parentPartition.Guid;
    }
}
