namespace Fargo.Core.Items;

/// <summary>
/// Represents a single period of the containment history of an item: which
/// container directly held the item during a given interval of time.
/// </summary>
/// <remarks>
/// <para>
/// This entity models <b>application-time temporal history</b>. Each instance is
/// one row that is valid during a half-open time interval
/// <c>[<see cref="PeriodStart"/>, <see cref="PeriodEnd"/>)</c> — inclusive of the
/// start instant and exclusive of the end instant. While a row is the current
/// state of the item, its <see cref="PeriodEnd"/> is open ("infinity"); when the
/// item's container changes, that row is closed (its <see cref="PeriodEnd"/> is
/// set to the instant of the change) and a new open row is created. An item
/// therefore has exactly one open row at any moment and a chain of closed rows
/// describing where it was in the past.
/// </para>
/// <para>
/// The adjacent, non-overlapping nature of these periods is a hard invariant:
/// for any single item, no two rows may have overlapping periods. This is
/// enforced in the database by a GiST exclusion constraint (see the remarks on
/// the infrastructure configuration and the migration that creates it).
/// </para>
/// <para>
/// <b>How rows are produced.</b> This history is <i>not</i> written by the
/// application layer. PostgreSQL has no built-in system versioning, so a database
/// trigger on the <c>items</c> table owns the entire lifecycle of these rows:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///     When an item is inserted, the trigger opens the first period, recording
///     the item's initial container (or the removed-from-containers state when
///     the item starts without a parent).
///     </description>
///   </item>
///   <item>
///     <description>
///     When an item's parent container actually changes, the trigger closes the
///     current open period at the instant of the change and opens a new period
///     reflecting the new container. An update that does not change the parent
///     container produces no history row.
///     </description>
///   </item>
/// </list>
/// <para>
/// Because the trigger is the single writer, this type is effectively read-only
/// from the domain's perspective: its setters are private and it is queried, not
/// mutated, by the application. To answer "which container held this item at a
/// given instant", select the row whose period contains that instant; to get the
/// full timeline, order the item's rows by <see cref="PeriodStart"/>.
/// </para>
/// <para>
/// The constructor exists so that Entity Framework (and the derived
/// infrastructure type) can materialize instances; it is not used to append
/// history from application code.
/// </para>
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
    /// Gets a value indicating whether this period represents the item being
    /// outside of any container.
    /// </summary>
    /// <remarks>
    /// This is <see langword="true"/> exactly when
    /// <see cref="ParentItemContainerGuid"/> is <see langword="null"/>, and lets
    /// a "removed from containers" period be distinguished without inspecting the
    /// parent identifier.
    /// </remarks>
    public bool RemovedFromContainers { get; private init; } = false;

    /// <summary>
    /// Gets the instant, in UTC, at which this period starts being valid
    /// (inclusive).
    /// </summary>
    /// <remarks>
    /// Corresponds to the lower bound of the underlying range column. It is the
    /// instant at which the item entered the container recorded by this row.
    /// </remarks>
    public virtual DateTimeOffset PeriodStart { get; }

    /// <summary>
    /// Gets the instant, in UTC, at which this period stops being valid
    /// (exclusive).
    /// </summary>
    /// <remarks>
    /// Corresponds to the upper bound of the underlying range column. While this
    /// row is the current state of the item the bound is open ("infinity"); it is
    /// set to a concrete instant by the database trigger when the item's container
    /// changes, closing the period. The half-open <c>[start, end)</c> semantics
    /// guarantee that a closed period and the period that follows it are adjacent
    /// but never overlap.
    /// </remarks>
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
    /// <remarks>
    /// This constructor exists for Entity Framework materialization and for the
    /// derived infrastructure type. History rows are created by the database
    /// trigger, not by constructing instances in application code. The validity
    /// period is assigned by the database (default or trigger) and is therefore
    /// not a constructor parameter.
    /// </remarks>
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
