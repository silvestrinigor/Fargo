using Fargo.Core.Partitions;

namespace Fargo.Core.Items;

/// <summary>
/// Represents an association between an item and a partition.
/// </summary>
/// <remarks>
/// This association defines a partition that is directly assigned to the item
/// and contributes to the item's partition scope.
/// </remarks>
public class ItemPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated item.
    /// </summary>
    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// Gets the associated item.
    /// </summary>
    public Item Item { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the associated partition.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the associated partition.
    /// </summary>
    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new association between the specified item and partition.
    /// </summary>
    /// <param name="item">The item to associate with the partition.</param>
    /// <param name="partition">The partition to associate with the item.</param>
    internal ItemPartition(Item item, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(partition);

        Item = item;
        ItemGuid = item.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new association between the specified item and a partition
    /// identified by its unique identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not need
    /// to be loaded, such as when associating the item with a well-known or
    /// global partition whose identifier is already known and stable.
    ///
    /// Prefer this constructor over the constructor that accepts a
    /// <see cref="Partition"/> when only the partition identifier is required.
    /// This avoids requiring the partition entity to be loaded solely for
    /// creating the association.
    /// </remarks>
    /// <param name="item">The item to associate with the partition.</param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
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
