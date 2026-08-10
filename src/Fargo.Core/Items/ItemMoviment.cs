using Fargo.Core.Entities;

namespace Fargo.Core.Items;

public class ItemMoviment : IEntity
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public Guid ItemMovedGuid { get; private init; }

    public Item ItemMoved { get; private init; }

    public Guid? ItemContainerPositionGuid { get; private init; }

    public Item? ItemContainerPosition { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemMoviment() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static ItemMoviment CreateItemContainerMoviment(Item itemMoved, Item? itemContainerPosition)
    {
        return new ItemMoviment
        {
            ItemMoved = itemMoved,
            ItemMovedGuid = itemMoved.Guid,

            ItemContainerPosition = itemContainerPosition,
            ItemContainerPositionGuid = itemContainerPosition?.Guid
        };
    }
}
