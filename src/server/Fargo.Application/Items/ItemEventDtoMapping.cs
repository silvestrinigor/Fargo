using Fargo.Core.Items.Events;

namespace Fargo.Application.Items;

/// <summary>
/// Provides mapping extensions for converting item event entities to
/// <see cref="ItemEventDto"/> data transfer objects.
/// </summary>
public static class ItemEventDtoMapping
{
    /// <summary>
    /// Converts a movement event entity to an <see cref="ItemEventDto"/>.
    /// </summary>
    /// <param name="movimentEvent">The movement event to convert.</param>
    /// <returns>A DTO populated from the movement event and its base event.</returns>
    public static ItemEventDto ToDto(this ItemMovimentEvent movimentEvent)
    {
        var itemEvent = movimentEvent.ItemEvent;

        return new ItemEventDto(
            itemEvent.Guid,
            itemEvent.ArticleGuid,
            itemEvent.EventType,
            itemEvent.OccuredAt,
            new ItemMovimentEventDto(
                movimentEvent.Quantity,
                movimentEvent.FromItemContainerGuid,
                movimentEvent.FromNoContainer,
                movimentEvent.ToItemContainerGuid,
                movimentEvent.ToNoContainer));
    }
}
