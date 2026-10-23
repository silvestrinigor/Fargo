using Fargo.Core.Items;
using NpgsqlTypes;

namespace Fargo.Infrastructure.Entities;

/// <summary>
/// PostgreSQL-specific persistence shape of <see cref="ItemParentContainerHistory"/>
/// that stores the validity period as a single <c>tstzrange</c> column.
/// </summary>
/// <remarks>
/// <para>
/// The domain type exposes the period as two separate instants
/// (<see cref="ItemParentContainerHistory.PeriodStart"/> and
/// <see cref="ItemParentContainerHistory.PeriodEnd"/>). PostgreSQL, however,
/// models application time as a native range type, and the GiST exclusion
/// constraint that forbids overlapping periods operates on that single range
/// column. This infrastructure type bridges the two: it holds the raw
/// <see cref="ValidPeriod"/> range and projects its bounds onto the domain's
/// period properties.
/// </para>
/// <para>
/// The range uses half-open <c>[lower, upper)</c> bounds. An open (current)
/// period has an upper bound of "infinity"; the database trigger on the
/// <c>items</c> table replaces that bound with the instant of the change when an
/// item's container changes, and inserts a new open row. Rows of this type are
/// consequently written only by the database, never appended from application
/// code.
/// </para>
/// </remarks>
public class ItemParentContainerHistoryPostgresTemporal : ItemParentContainerHistory
{
    /// <summary>
    /// Gets or sets the surrogate primary key of the history row.
    /// </summary>
    /// <remarks>
    /// A surrogate key is used because an item has many history rows over time;
    /// uniqueness across time for a given item is enforced separately by the GiST
    /// exclusion constraint on (item, period), not by this key.
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the validity period of this row as a PostgreSQL
    /// <c>tstzrange</c> (timestamp-with-time-zone range).
    /// </summary>
    /// <remarks>
    /// The lower bound is the instant the item entered the recorded container; the
    /// upper bound is "infinity" while the row is current, or the instant of the
    /// next container change once the period has been closed. The column default
    /// (<c>tstzrange(now(), 'infinity', '[)')</c>) and the trigger both produce
    /// half-open ranges so that consecutive periods are adjacent without
    /// overlapping.
    /// </remarks>
    public NpgsqlRange<DateTimeOffset> ValidPeriod { get; set; }

    /// <inheritdoc/>
    public override DateTimeOffset PeriodStart => ValidPeriod.LowerBound;

    /// <inheritdoc/>
    public override DateTimeOffset PeriodEnd => ValidPeriod.UpperBound;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ItemParentContainerHistoryPostgresTemporal"/> class.
    /// </summary>
    /// <param name="itemGuid">
    /// The unique identifier of the item whose containment history is recorded.
    /// </param>
    /// <param name="parentItemContainerGuid">
    /// The unique identifier of the container item, or <see langword="null"/>
    /// when the item is not placed inside another item.
    /// </param>
    /// <remarks>
    /// Intended for Entity Framework materialization only. The validity period is
    /// supplied by the database, so it is not a constructor parameter.
    /// </remarks>
    public ItemParentContainerHistoryPostgresTemporal(Guid itemGuid, Guid? parentItemContainerGuid) : base(itemGuid, parentItemContainerGuid)
    {
    }
}
