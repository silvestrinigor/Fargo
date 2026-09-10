namespace Fargo.Core.Items;

public class ItemParentContainerHistory
{
    public Item Item { get; private init; } = null!;

    public Guid ItemGuid { get; private init; }

    public Item? ParentItemContainer { get; private init; }

    public Guid? ParentItemContainerGuid { get; private init; }

    public bool RemovedFromContainers { get; private init; } = false;

    public virtual DateTimeOffset PeriodStart { get; }

    public virtual DateTimeOffset PeriodEnd { get; }

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
