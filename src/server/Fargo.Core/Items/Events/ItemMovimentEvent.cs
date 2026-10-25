using Fargo.Core.Articles;

namespace Fargo.Core.Items.Events;

public class ItemMovimentEvent
{
    public Guid ItemEventGuid { get; private init; }

    public ItemEvent ItemEvent { get; private init; }

    public Guid? FromItemContainerGuid { get; private init; }

    public Item? FromItemContainer { get; private init; }

    // TODO: Doc. Useful to know when the item was not inside a container, because if the FromItemContainer is deleted, is set null. Propably need a better property name.
    public bool FromNoContainer { get; private init; }

    public Guid? ToItemContainerGuid { get; private init; }

    public Item? ToItemContainer { get; private init; }

    // TODO: Doc. Useful to know when the item was moved outside all containers, because if the ToItemContainer is deleted, is set null. Propably need a better property name.
    public bool ToNoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemMovimentEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ItemMovimentEvent(Article article)
    {
        var itemEvent = new ItemEvent(article, ItemEventType.Moviment);

        ItemEvent = itemEvent;
        ItemEventGuid = itemEvent.Guid;
    }

    public ItemMovimentEvent NewItemMovimentEvent(Article article, Item? fromContainer, Item? toContainer)
    {
        // TODO: Check if is container
        // TODO: Check if position is changed (fromContainer cannot be equal toContainer)

        var itemMovimentEvent = new ItemMovimentEvent(article)
        {
            FromItemContainer = fromContainer,
            FromItemContainerGuid = fromContainer?.Guid,
            FromNoContainer = fromContainer == null,

            ToItemContainer = toContainer,
            ToItemContainerGuid = toContainer?.Guid,
            ToNoContainer = toContainer == null,
        };

        return itemMovimentEvent;
    }
}
