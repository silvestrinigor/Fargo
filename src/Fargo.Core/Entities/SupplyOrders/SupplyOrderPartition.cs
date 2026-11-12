using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// Represents an association between a supply order and a partition.
/// </summary>
/// <remarks>
/// This association defines a partition that is directly assigned to the supply
/// order and contributes to the order's partition scope.
/// </remarks>
public class SupplyOrderPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated supply order.
    /// </summary>
    public Guid SupplyOrderGuid { get; private init; }

    /// <summary>
    /// Gets the associated supply order.
    /// </summary>
    public SupplyOrder SupplyOrder { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the associated partition.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the associated partition.
    /// </summary>
    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private SupplyOrderPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new association between the specified supply order and partition.
    /// </summary>
    /// <param name="supplyOrder">The supply order to associate with the partition.</param>
    /// <param name="partition">The partition to associate with the supply order.</param>
    internal SupplyOrderPartition(SupplyOrder supplyOrder, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(supplyOrder);
        ArgumentNullException.ThrowIfNull(partition);

        SupplyOrder = supplyOrder;
        SupplyOrderGuid = supplyOrder.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new association between the specified supply order and a
    /// partition identified by its unique identifier.
    /// </summary>
    /// <remarks>
    /// Prefer this constructor when only the partition identifier is required
    /// (for example when assigning the well-known global partition), so the
    /// partition entity does not need to be loaded solely to create the association.
    /// </remarks>
    /// <param name="supplyOrder">The supply order to associate with the partition.</param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition. This should be a known, valid
    /// partition identifier, such as the identifier of the global partition.
    /// </param>
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
