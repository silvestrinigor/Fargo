using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// A partition directly assigned to a supply order, contributing to its partition scope.
/// </summary>
public class SupplyOrderPartition
{
    public Guid SupplyOrderGuid { get; private init; }

    public SupplyOrder SupplyOrder { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private SupplyOrderPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal SupplyOrderPartition(SupplyOrder supplyOrder, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(supplyOrder);
        ArgumentNullException.ThrowIfNull(partition);

        SupplyOrder = supplyOrder;
        SupplyOrderGuid = supplyOrder.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    // Takes only the identifier so the partition need not be loaded, e.g. for the global partition.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal SupplyOrderPartition(SupplyOrder supplyOrder, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(supplyOrder);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        SupplyOrder = supplyOrder;
        SupplyOrderGuid = supplyOrder.Guid;

        PartitionGuid = partitionGuid;
    }
}
