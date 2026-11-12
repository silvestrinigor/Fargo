using Fargo.Core.Common;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Partitions;

/// <summary>
/// A node of the hierarchy that scopes access to entities.
/// </summary>
/// <remarks>
/// Access flows down the hierarchy: access to a partition grants access to the entities of its
/// descendants, never of its parent or of other branches.
///
/// A single global partition is the root of the hierarchy. Access to it is restricted to highly
/// privileged users.
/// </remarks>
public class Partition : Entity, IConcurrencyVersioned
{
    public bool IsGlobalPartition => Guid == FargoCoreWellKnowGuids.GlobalPartitionGuid;

    public required Name Name { get; set; }

    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// <see langword="null"/> only for the global partition, the root of the hierarchy.
    /// </summary>
    public Guid? ParentPartitionGuid { get; private set; }

    public Partition? ParentPartition { get; private set; }

    // Required by EF Core and the factory methods.
    private Partition()
    {
        EntityType = EntityType.Partition;
    }

    public static Partition CreatePartition(Name name, Partition parentPartition)
    {
        var partition = new Partition
        {
            Name = name
        };

        partition.SetParentPartition(parentPartition);

        return partition;
    }

    public static Partition CreateGlobalPartition(Name name)
    {
        var globalPartition = new Partition
        {
            Guid = FargoCoreWellKnowGuids.GlobalPartitionGuid,
            Name = name
        };

        return globalPartition;
    }

    /// <summary>
    /// Does <b>not</b> detect circular hierarchies; validate with
    /// <see cref="PartitionService"/> first.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The partition is the global partition, or <paramref name="parentPartition"/> is itself.
    /// </exception>
    public void SetParentPartition(Partition parentPartition)
    {
        ArgumentNullException.ThrowIfNull(parentPartition);

        if (IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' cannot have a parent partition.", FargoErrorType.InvalidOperation);
        }

        if (parentPartition.Guid == Guid)
        {
            throw new FargoCoreException(
                $"The partition partition '{Guid}' cannot be its own parent.", FargoErrorType.InvalidOperation);
        }

        ParentPartition = parentPartition;

        ParentPartitionGuid = parentPartition.Guid;
    }

    /// <summary>
    /// Checks only what this partition can tell on its own; the presence of child partitions is
    /// checked by <see cref="PartitionService"/>.
    /// </summary>
    /// <exception cref="FargoCoreException">The partition is the global partition.</exception>
    public void ValidateCanBeDeleted()
    {
        if (IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' cannot be deleted.",
                FargoErrorType.InvalidOperation);
        }
    }
}
