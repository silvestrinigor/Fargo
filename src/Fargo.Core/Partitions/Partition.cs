using Fargo.Core.Entities;
using Fargo.Core.Shared.Informations;

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
public class Partition : IEntity
{
    /// <summary>
    /// Gets the unique identifier of the partition.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets a value indicating whether this is the global partition.
    /// </summary>
    public bool IsGlobalPartition => Guid == FargoCoreGuids.GlobalPartitionGuid;

    /// <summary>
    /// Gets or sets the name of the partition.
    /// </summary>
    public required Name Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the partition.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the unique identifier of the parent partition, if any.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the partition is the
    /// global partition, which is the root of the partition hierarchy.
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Partition"/> class.
    /// Intended only for factory methods and Entity Framework.
    /// </summary>
    private Partition()
    {
    }

    /// <summary>
    /// Creates a new partition.
    /// </summary>
    /// <param name="name">The name of the partition.</param>
    /// <param name="parentPartition">
    /// The parent of the newly created partition.
    /// </param>
    /// <returns>A new <see cref="Partition"/> instance.</returns>
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
    /// Creates the global partition.
    /// </summary>
    /// <param name="name">The name of the global partition.</param>
    /// <returns>The global <see cref="Partition"/>.</returns>
    public static Partition CreateGlobalPartition(Name name)
    {
        var globalPartition = new Partition
        {
            Guid = FargoCoreGuids.GlobalPartitionGuid,
            Name = name
        };

        return globalPartition;
    }

    /// <summary>
    /// Assigns the specified partition as the parent of the current partition.
    /// </summary>
    /// <param name="parentPartition">
    /// The partition to assign as the parent.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to assign a parent to the global partition or when
    /// attempting to assign the partition as its own parent.
    /// </exception>
    public void SetParentPartition(Partition parentPartition)
    {
        if (IsGlobalPartition)
        {
            throw new FargoCoreException(
                "The global partition cannot have a parent partition.", FargoCoreErrorType.InvalidOperation);
        }

        if (parentPartition.Guid == Guid)
        {
            throw new FargoCoreException(
                "A partition cannot be its own parent.", FargoCoreErrorType.InvalidArgument);
        }

        ParentPartition = parentPartition;

        ParentPartitionGuid = parentPartition.Guid;
    }
}
