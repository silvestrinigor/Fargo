using Fargo.Core.Entities.Items;
using NpgsqlTypes;

namespace Fargo.Infrastructure.Persistence.Entities;

/// <summary>
/// Stores the validity period as a single <c>tstzrange</c> column, which the GiST exclusion constraint
/// against overlapping periods needs, and projects its bounds onto the domain's start and end.
/// </summary>
/// <remarks>
/// The range is half-open <c>[lower, upper)</c>. A current period has an upper bound of "infinity"; the
/// trigger on <c>items</c> closes it at the instant of a container change and inserts a new open row. Rows
/// are therefore written only by the database, never appended from application code.
/// </remarks>
public class ItemParentContainerHistoryPostgresTemporal : ItemParentContainerHistory
{
    /// <summary>
    /// Surrogate key: an item has many history rows over time, and non-overlap per item is enforced by the
    /// exclusion constraint, not by this key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The upper bound is "infinity" while the row is current, and the instant of the next container change
    /// once closed.
    /// </summary>
    public NpgsqlRange<DateTimeOffset> ValidPeriod { get; set; }

    public override DateTimeOffset PeriodStart => ValidPeriod.LowerBound;

    public override DateTimeOffset PeriodEnd => ValidPeriod.UpperBound;

    // Used by EF Core to materialize rows; the period is supplied by the database.
    public ItemParentContainerHistoryPostgresTemporal(Guid itemGuid, Guid? parentItemContainerGuid) : base(itemGuid, parentItemContainerGuid)
    {
    }
}
