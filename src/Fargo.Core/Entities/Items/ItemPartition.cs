using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Items;

/// <summary>
/// A partition directly assigned to an item; it contributes to the item's partition scope.
/// </summary>
public class ItemPartition
{
    public Guid ItemGuid { get; private init; }

    public Item Item { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ItemPartition(Item item, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(partition);

        Item = item;
        ItemGuid = item.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    // Prefer this overload when only the identifier is known (for example the global partition),
    // so the partition entity does not have to be loaded just to create the association.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ItemPartition(Item item, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(item);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        Item = item;
        ItemGuid = item.Guid;

        PartitionGuid = partitionGuid;
    }
}
