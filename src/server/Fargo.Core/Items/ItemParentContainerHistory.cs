namespace Fargo.Core.Items;

/// <summary>
/// Represents the historical record of the container that directly contains an item.
/// </summary>
/// <remarks>
/// This entity is stored as a temporal table, allowing the containment history of
/// an item to be queried as of any point in time.
/// </remarks>
public class ItemParentContainerHistory
{
    /// <summary>
    /// Gets the item whose containment history is recorded.
    /// </summary>
    public Item Item { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the item whose containment history is recorded.
    /// </summary>
    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// Gets the container item that directly contained the item.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the item was not placed inside
    /// another item.
    /// </remarks>
    public Item? ParentItemContainer { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the container item that directly contained
    /// the item.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the item was not placed inside
    /// another item.
    /// </remarks>
    public Guid? ParentItemContainerGuid { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the item was removed from its containers.
    /// </summary>
    public bool RemovedFromContainers { get; private init; } = false;

    /// <summary>
    /// Gets the date and time, in UTC, at which this historical record starts being valid.
    /// </summary>
    public virtual DateTimeOffset PeriodStart { get; }

    /// <summary>
    /// Gets the date and time, in UTC, at which this historical record stops being valid.
    /// </summary>
    public virtual DateTimeOffset PeriodEnd { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemParentContainerHistory"/> class.
    /// </summary>
    /// <param name="itemGuid">
    /// The unique identifier of the item whose containment history is recorded.
    /// </param>
    /// <param name="parentItemContainerGuid">
    /// The unique identifier of the container item that directly contains the item,
    /// or <see langword="null"/> when the item is not placed inside another item.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public ItemParentContainerHistory(Guid itemGuid, Guid? parentItemContainerGuid)
    {
        if (itemGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "The item identifier cannot be empty.",
                nameof(itemGuid));
        }

        ItemGuid = itemGuid;

        ParentItemContainerGuid = parentItemContainerGuid;

        if (ParentItemContainerGuid is null)
        {
            RemovedFromContainers = true;
        }
    }
}
