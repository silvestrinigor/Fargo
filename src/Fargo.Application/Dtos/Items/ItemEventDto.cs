using Fargo.Core.Events;
using UnitsNet;

namespace Fargo.Application.Dtos.Items;

/// <param name="ItemGuid">
/// <see langword="null"/> for an event-tracked article. For an item, it stays after the item is deleted.
/// </param>
/// <param name="FromItemContainerGuid">
/// <see langword="null"/> when there was no origin container or it has since been deleted; see
/// <paramref name="FromNoContainer"/>.
/// </param>
/// <param name="ToItemContainerGuid">
/// <see langword="null"/> when there was no destination container or it has since been deleted; see
/// <paramref name="ToNoContainer"/>.
/// </param>
public sealed record ItemEventDto(
    Guid Guid,
    Guid ArticleGuid,
    ItemEventType EventType,
    DateTimeOffset OccuredAt,
    Guid? ItemGuid,
    Scalar Quantity,
    Guid? FromItemContainerGuid,
    bool FromNoContainer,
    Guid? ToItemContainerGuid,
    bool ToNoContainer
);

public static class ItemEventDtoMapping
{
    public static ItemEventDto ToDto(this ItemEvent itemEvent)
    {
        return new ItemEventDto(
            itemEvent.Guid,
            itemEvent.ArticleGuid,
            itemEvent.ItemEventType,
            itemEvent.OccuredAt,
            itemEvent.ItemGuid,
            itemEvent.Quantity,
            itemEvent.FromItemContainerGuid,
            itemEvent.FromNoContainer,
            itemEvent.ToItemContainerGuid,
            itemEvent.ToNoContainer);
    }
}