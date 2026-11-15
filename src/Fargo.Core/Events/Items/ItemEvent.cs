using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using UnitsNet;

namespace Fargo.Core.Events;

/// <summary>
/// A quantity of an article that entered stock, left stock, or moved between containers, always
/// stored as a movement from an origin to a destination.
/// </summary>
/// <remarks>
/// <para>
/// Event-tracked articles have no per-unit <see cref="Item"/>, so clients record their stock with
/// the factories on this type and <see cref="ItemGuid"/> is <see langword="null"/>.
/// </para>
/// <para>
/// For entity-tracked articles the application records an event when an <see cref="Item"/> is
/// created, moved or deleted, with the item factories on this type. Deleting a container deletes the
/// items inside it, so each of them gets its own deletion event.
/// </para>
/// <para>
/// A creation has no origin, a deletion has no destination, and a movement can have either side or both.
/// </para>
/// </remarks>
public class ItemEvent : Event
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public ItemEventType ItemEventType { get; private init; }

    /// <summary>
    /// <see langword="null"/> for an event-tracked article. Not a foreign key, so the event stays after the item is deleted.
    /// </summary>
    public Guid? ItemGuid { get; private init; }

    /// <summary>
    /// Always strictly positive; direction comes from origin and destination, not from the sign. A
    /// <see cref="Scalar"/>, so fractional and bulk amounts are allowed. It is 1 for an item event.
    /// </summary>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// <see langword="null"/> when there was no origin container, or when it was deleted later.
    /// Use <see cref="FromNoContainer"/> to tell the two cases apart.
    /// </summary>
    public Guid? FromItemContainerGuid { get; private init; }

    public Item? FromItemContainer { get; private init; }

    /// <summary>
    /// Whether the event originally had no origin container. Do not infer it from
    /// <see cref="FromItemContainerGuid"/>, which becomes <see langword="null"/> if the container is deleted.
    /// </summary>
    public bool FromNoContainer { get; private init; }

    /// <summary>
    /// <see langword="null"/> when there was no destination container, or when it was deleted later.
    /// Use <see cref="ToNoContainer"/> to tell the two cases apart.
    /// </summary>
    public Guid? ToItemContainerGuid { get; private init; }

    public Item? ToItemContainer { get; private init; }

    /// <summary>
    /// Whether the event originally had no destination container. Do not infer it from
    /// <see cref="ToItemContainerGuid"/>, which becomes <see langword="null"/> if the container is deleted.
    /// </summary>
    public bool ToNoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
    private ItemEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private ItemEvent(
        Article article,
        ItemEventType eventType,
        Scalar quantity,
        Item? fromContainer,
        Item? toContainer,
        bool fromNoContainer,
        bool toNoContainer,
        DateTimeOffset? occuredAt)
    {
        ArgumentNullException.ThrowIfNull(article);

        if (article.TrackingMode != ArticleTrackingMode.Event)
        {
            throw new FargoCoreException(
                $"An item event cannot be recorded for article '{article.Guid}' because it is not event-tracked.",
                FargoErrorType.InvalidOperation);
        }

        if (quantity.Amount <= 0)
        {
            throw new FargoCoreException(
                "The quantity of an item event must be strictly positive.",
                FargoErrorType.InvalidOperation);
        }

        ThrowIfNotContainer(fromContainer);
        ThrowIfNotContainer(toContainer);

        EventType = EventType.Item;

        ArticleGuid = article.Guid;
        Article = article;

        ItemEventType = eventType;

        Quantity = quantity;

        FromItemContainer = fromContainer;
        FromItemContainerGuid = fromContainer?.Guid;
        FromNoContainer = fromNoContainer;

        ToItemContainer = toContainer;
        ToItemContainerGuid = toContainer?.Guid;
        ToNoContainer = toNoContainer;

        if (occuredAt is { } instant)
        {
            OccuredAt = instant;
        }
    }

    // For an item: the article is not loaded, only its identifier is needed.
    private ItemEvent(
        Guid itemGuid,
        Guid articleGuid,
        ItemEventType eventType,
        Guid? fromContainerGuid,
        Guid? toContainerGuid,
        bool fromNoContainer,
        bool toNoContainer)
    {
        EventType = EventType.Item;

        ArticleGuid = articleGuid;

        ItemEventType = eventType;

        ItemGuid = itemGuid;

        Quantity = Scalar.FromAmount(1);

        FromItemContainerGuid = fromContainerGuid;
        FromNoContainer = fromNoContainer;

        ToItemContainerGuid = toContainerGuid;
        ToNoContainer = toNoContainer;
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
        if (fromContainer is null && toContainer is null)
        {
            throw new FargoCoreException(
                "A movement event must have an origin container, a destination container, or both.",
                FargoErrorType.InvalidOperation);
        }

        if (fromContainer is not null
            && toContainer is not null
            && fromContainer.Guid == toContainer.Guid)
        {
            throw new FargoCoreException(
                $"A movement event cannot have the same origin and destination container '{fromContainer.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        return new ItemEvent(
            article,
            ItemEventType.Moviment,
            quantity,
            fromContainer,
            toContainer,
            fromNoContainer: fromContainer is null,
            toNoContainer: toContainer is null,
            occuredAt);
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
        => new(
            article,
            ItemEventType.Creation,
            quantity,
            fromContainer: null,
            toContainer: container,
            fromNoContainer: true,
            toNoContainer: container is null,
            occuredAt);

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
        => new(
            article,
            ItemEventType.Deletion,
            quantity,
            fromContainer: container,
            toContainer: null,
            fromNoContainer: container is null,
            toNoContainer: true,
            occuredAt);

    /// <summary>
    /// Records the creation of <paramref name="item"/>, inside its parent container if it has one.
    /// </summary>
    /// <remarks>
    /// Call it after the item is placed in its first container.
    /// </remarks>
    public static ItemEvent NewItemCreatedEvent(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemEvent(
            item.Guid,
            item.ArticleGuid,
            ItemEventType.Creation,
            fromContainerGuid: null,
            toContainerGuid: item.ParentItemContainerGuid,
            fromNoContainer: true,
            toNoContainer: item.ParentItemContainerGuid is null);
    }

    /// <summary>
    /// Records that <paramref name="item"/> moved from <paramref name="fromContainerGuid"/> to its
    /// current parent container, or out of all containers.
    /// </summary>
    /// <remarks>
    /// Call it after the item is moved, with the parent it had before.
    /// </remarks>
    public static ItemEvent NewItemMovedEvent(Item item, Guid? fromContainerGuid)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemEvent(
            item.Guid,
            item.ArticleGuid,
            ItemEventType.Moviment,
            fromContainerGuid,
            toContainerGuid: item.ParentItemContainerGuid,
            fromNoContainer: fromContainerGuid is null,
            toNoContainer: item.ParentItemContainerGuid is null);
    }

    /// <summary>
    /// Records the deletion of <paramref name="item"/> from its parent container, if it has one.
    /// </summary>
    public static ItemEvent NewItemDeletedEvent(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemEvent(
            item.Guid,
            item.ArticleGuid,
            ItemEventType.Deletion,
            fromContainerGuid: item.ParentItemContainerGuid,
            toContainerGuid: null,
            fromNoContainer: item.ParentItemContainerGuid is null,
            toNoContainer: true);
    }

    /// <summary>
    /// Records the deletion of an item that was inside a container that is deleted with it.
    /// </summary>
    /// <remarks>
    /// The container no longer exists once the deletion is saved, so the event keeps no reference to it.
    /// </remarks>
    public static ItemEvent NewContainedItemDeletedEvent(Guid itemGuid, Guid articleGuid)
        => new(
            itemGuid,
            articleGuid,
            ItemEventType.Deletion,
            fromContainerGuid: null,
            toContainerGuid: null,
            fromNoContainer: false,
            toNoContainer: true);

    private static void ThrowIfNotContainer(Item? container)
    {
        if (container is not null && container.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{container.Guid}' is not a container item.",
                FargoErrorType.InvalidOperation);
        }
    }
}
