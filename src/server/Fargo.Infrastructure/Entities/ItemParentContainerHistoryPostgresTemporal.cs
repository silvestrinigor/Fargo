using Fargo.Core.Items;
using NpgsqlTypes;

namespace Fargo.Infrastructure.Entities;

public class ItemParentContainerHistoryPostgresTemporal : ItemParentContainerHistory
{
    public int Id { get; set; }

    public NpgsqlRange<DateTimeOffset> ValidPeriod { get; set; }

    public override DateTimeOffset PeriodStart => ValidPeriod.LowerBound;

    public override DateTimeOffset PeriodEnd => ValidPeriod.UpperBound;

    public ItemParentContainerHistoryPostgresTemporal(Guid itemGuid, Guid? parentItemContainerGuid) : base(itemGuid, parentItemContainerGuid)
    {
    }
}
