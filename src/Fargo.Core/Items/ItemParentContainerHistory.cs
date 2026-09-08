namespace Fargo.Core.Items;

public class ItemParentContainerHistory
{
    public Guid? ParentItemContainerGuid { get; private init; }

    public Guid ItemGuid { get; private init; }

    public DateTime ValidAt { get; private set; }

    private ItemParentContainerHistory()
    {
    }

    public ItemParentContainerHistory(Item item)
    {
        ItemGuid = item.Guid;

        ParentItemContainerGuid = item.ParentItemContainerGuid;
    }
}
