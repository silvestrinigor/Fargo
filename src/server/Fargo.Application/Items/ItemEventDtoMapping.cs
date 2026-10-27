using Fargo.Core.Items.Events;

namespace Fargo.Application.Items;

/// <summary>
/// Provides mapping extensions for converting item event entities to
/// <see cref="ItemEventDto"/> data transfer objects.
/// </summary>
public static class ItemEventDtoMapping
{
    /// <summary>
    /// Converts an item event entity to an <see cref="ItemEventDto"/>.
    /// </summary>
    /// <param name="itemEvent">The item event to convert.</param>
    /// <returns>A DTO populated from the event and its payload, if any.</returns>
    public static ItemEventDto ToDto(this ItemEvent itemEvent)
    {
        var moviment = itemEvent.Moviment;
        var lifecycle = itemEvent.Lifecycle;

        return new ItemEventDto(
            itemEvent.Guid,
            itemEvent.ArticleGuid,
            itemEvent.EventType,
            itemEvent.OccuredAt,
            moviment is null
                ? null
                : new ItemMovimentEventDto(
                    moviment.Quantity,
                    moviment.FromItemContainerGuid,
                    moviment.FromNoContainer,
                    moviment.ToItemContainerGuid,
                    moviment.ToNoContainer),
            lifecycle is null
                ? null
                : new ItemLifecycleEventDto(
                    lifecycle.Quantity,
                    lifecycle.ItemContainerGuid,
                    lifecycle.NoContainer));
    }
}
