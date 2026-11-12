namespace Fargo.Core.Entities.Items;

/// <summary>
/// One period of an item's containment history: which container directly held the item during a
/// half-open interval <c>[<see cref="PeriodStart"/>, <see cref="PeriodEnd"/>)</c>.
/// </summary>
/// <remarks>
/// <para>
/// Rows are written only by a database trigger on the <c>items</c> table, because PostgreSQL has
/// no built-in system versioning; the application only reads them. On insert the trigger opens
/// the first period. When the parent container actually changes it closes the open period at the
/// instant of the change and opens a new one; an update that leaves the parent unchanged writes
/// nothing. An item therefore has exactly one open row (<see cref="PeriodEnd"/> is "infinity")
/// plus a chain of closed ones.
/// </para>
/// <para>
/// Periods of one item must never overlap; a GiST exclusion constraint enforces this in the
/// database. To find the container at an instant, select the row whose period contains it; for
/// the timeline, order by <see cref="PeriodStart"/>.
/// </para>
/// </remarks>
public class ItemParentContainerHistory
{
    public Item Item { get; private init; } = null!;

    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// <see langword="null"/> when the item was not placed inside another item.
    /// </summary>
    public Item? ParentItemContainer { get; private init; }

    /// <summary>
    /// <see langword="null"/> when the item was not placed inside another item.
    /// </summary>
    public Guid? ParentItemContainerGuid { get; private init; }

    /// <summary>
    /// <see langword="true"/> exactly when <see cref="ParentItemContainerGuid"/> is
    /// <see langword="null"/>.
    /// </summary>
    public bool RemovedFromContainers { get; private init; } = false;

    /// <summary>
    /// UTC instant at which the item entered the recorded container (inclusive lower bound).
    /// </summary>
    public virtual DateTimeOffset PeriodStart { get; }

    /// <summary>
    /// UTC exclusive upper bound; open ("infinity") while this row is the item's current state.
    /// </summary>
    public virtual DateTimeOffset PeriodEnd { get; }

    // Exists for EF Core and the derived infrastructure type; the period is assigned by the
    // database, so it is not a parameter.
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
