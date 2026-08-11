using Fargo.Core.Common;
using Fargo.Core.Entities;

namespace Fargo.Core.Items;

public class ItemParentContainerHistory : IEntity
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public Guid ItemGuid { get; private init; }

    public Item Item { get; private init; }

    public Guid? ParentItemContianerGuid { get; private init; }

    public Item? ParentItemContainer { get; private init; }

    public DateTimeOffsetRange ValidAt { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemParentContainerHistory() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static ItemParentContainerHistory CreateItemParentContainerHistory(Item item, Item? parentItemContainer, DateTimeOffsetRange validAt)
    {
        return new ItemParentContainerHistory
        {
            Item = item,
            ItemGuid = item.Guid,
            ParentItemContainer = parentItemContainer,
            ParentItemContianerGuid = parentItemContainer?.Guid,
            ValidAt = validAt
        };
    }
}
