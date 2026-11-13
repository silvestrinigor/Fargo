using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using UnitsNet;

namespace Fargo.Core.Events;

/// <summary>
/// Base record of the stock-event stream of an event-tracked <see cref="Article"/>, which has no
/// per-unit <see cref="Item"/> rows. The payload is in <see cref="Moviment"/> or
/// <see cref="Lifecycle"/>, depending on <see cref="ItemEventType"/>.
/// </summary>
/// <remarks>
/// Can only be created for an article whose <see cref="Article.TrackingMode"/> is
/// <see cref="ArticleTrackingMode.Event"/>.
/// </remarks>
public class ItemEvent : Event
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public ItemEventType ItemEventType { get; private init; }

    /// <summary>
    /// The movement payload; <see langword="null"/> unless the type is
    /// <see cref="ItemEventType.Moviment"/>.
    /// </summary>
    public ItemMovimentEvent? Moviment { get; private set; }

    /// <summary>
    /// The creation/deletion payload; <see langword="null"/> unless the type is
    /// <see cref="ItemEventType.Creation"/> or <see cref="ItemEventType.Deletion"/>.
    /// </summary>
    public ItemLifecycleEvent? Lifecycle { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
    private ItemEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ItemEvent(Article article, ItemEventType eventType, DateTimeOffset? occuredAt = null)
    {
        ArgumentNullException.ThrowIfNull(article);

        if (article.TrackingMode != ArticleTrackingMode.Event)
        {
            throw new FargoCoreException(
                $"An item event cannot be recorded for article '{article.Guid}' because it is not event-tracked.",
                FargoErrorType.InvalidOperation);
        }

        EventType = EventType.Item;

        ArticleGuid = article.Guid;
        Article = article;

        ItemEventType = eventType;

        if (occuredAt is { } instant)
        {
            OccuredAt = instant;
        }
    }

    /// <summary>
    /// Records a movement of <paramref name="quantity"/> between containers; a <see langword="null"/>
    /// container means outside any container. A null <paramref name="occuredAt"/> means now (UTC).
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The article is not event-tracked, the quantity is not strictly positive, origin and destination
    /// are the same container or both missing, or a container is not a container-type article.
    /// </exception>
    public static ItemEvent NewMovimentEvent(
        Article article,
        Item? fromContainer,
        Item? toContainer,
        Scalar quantity,
        DateTimeOffset? occuredAt = null)
    {
        var itemEvent = new ItemEvent(article, ItemEventType.Moviment, occuredAt);

        var moviment = ItemMovimentEvent.NewItemMovimentEvent(
            article, fromContainer, toContainer, quantity);

        moviment.ItemEventGuid = itemEvent.Guid;

        itemEvent.Moviment = moviment;

        return itemEvent;
    }

    /// <summary>
    /// Records the creation of <paramref name="quantity"/> of stock, optionally inside a container.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The article is not event-tracked, the quantity is not strictly positive, or the container is not
    /// a container-type article.
    /// </exception>
    public static ItemEvent NewCreationEvent(
        Article article,
        Item? container,
        Scalar quantity,
        DateTimeOffset? occuredAt = null)
        => NewLifecycleEvent(article, ItemEventType.Creation, container, quantity, occuredAt);

    /// <summary>
    /// Records the deletion of <paramref name="quantity"/> of stock, optionally from a container.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The article is not event-tracked, the quantity is not strictly positive, or the container is not
    /// a container-type article.
    /// </exception>
    public static ItemEvent NewDeletionEvent(
        Article article,
        Item? container,
        Scalar quantity,
        DateTimeOffset? occuredAt = null)
        => NewLifecycleEvent(article, ItemEventType.Deletion, container, quantity, occuredAt);

    private static ItemEvent NewLifecycleEvent(
        Article article,
        ItemEventType eventType,
        Item? container,
        Scalar quantity,
        DateTimeOffset? occuredAt)
    {
        var itemEvent = new ItemEvent(article, eventType, occuredAt);

        var lifecycle = ItemLifecycleEvent.NewItemLifecycleEvent(article, container, quantity);

        lifecycle.ItemEventGuid = itemEvent.Guid;

        itemEvent.Lifecycle = lifecycle;

        return itemEvent;
    }
}
