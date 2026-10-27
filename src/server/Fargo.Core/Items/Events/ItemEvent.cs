using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using UnitsNet;

namespace Fargo.Core.Items.Events;

/// <summary>
/// Represents an event recorded against an event-tracked <see cref="Article"/>.
/// </summary>
/// <remarks>
/// Event-based tracking does not materialize one <see cref="Item"/> row per
/// physical unit. Instead, changes to the stock of an article are recorded as a
/// stream of events. An <see cref="ItemEvent"/> is the base record of that
/// stream: it identifies the article the event concerns, the kind of event, and
/// the instant it occurred.
///
/// The concrete payload of an event is modelled by a related entity such as
/// <see cref="ItemMovimentEvent"/>, which the event references through its
/// <see cref="Moviment"/> navigation.
///
/// An <see cref="ItemEvent"/> can only be created for an article whose
/// <see cref="Article.TrackingMode"/> is <see cref="ArticleTrackingMode.Event"/>.
/// </remarks>
public class ItemEvent : IEntity, IEntityTyped
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the unique identifier of the <see cref="Article"/> the event concerns.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article the event concerns.
    /// </summary>
    /// <remarks>
    /// The associated article is always event-tracked
    /// (<see cref="ArticleTrackingMode.Event"/>).
    /// </remarks>
    public Article Article { get; private init; }

    /// <summary>
    /// Gets the kind of event represented by this record.
    /// </summary>
    public ItemEventType EventType { get; private init; }

    /// <summary>
    /// Gets the movement payload of this event, when its <see cref="EventType"/>
    /// is <see cref="ItemEventType.Moviment"/>; otherwise <see langword="null"/>.
    /// </summary>
    public ItemMovimentEvent? Moviment { get; private set; }

    /// <summary>
    /// Gets the creation/deletion payload of this event, when its
    /// <see cref="EventType"/> is <see cref="ItemEventType.Creation"/> or
    /// <see cref="ItemEventType.Deletion"/>; otherwise <see langword="null"/>.
    /// </summary>
    public ItemLifecycleEvent? Lifecycle { get; private set; }

    /// <summary>
    /// Gets the instant, in UTC, at which the event occurred.
    /// </summary>
    /// <remarks>
    /// This value drives point-in-time inventory reconstruction: an
    /// event-based inventory evaluated at an instant considers only events whose
    /// <see cref="OccuredAt"/> is at or before that instant. It defaults to the
    /// moment the event is created but may be set explicitly to back-date an
    /// event (for example, when importing historical movements).
    /// </remarks>
    public DateTimeOffset OccuredAt { get; private init; } = DateTimeOffset.UtcNow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new event entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ItemEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new event for the specified event-tracked article.
    /// </summary>
    /// <param name="article">The article the event concerns.</param>
    /// <param name="eventType">The kind of event.</param>
    /// <param name="occuredAt">
    /// The instant, in UTC, at which the event occurred. When
    /// <see langword="null"/>, the current UTC instant is used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when <paramref name="article"/> is not event-tracked
    /// (<see cref="ArticleTrackingMode.Event"/>).
    /// </exception>
    internal ItemEvent(Article article, ItemEventType eventType, DateTimeOffset? occuredAt = null)
    {
        ArgumentNullException.ThrowIfNull(article);

        if (article.TrackingMode != ArticleTrackingMode.Event)
        {
            throw new FargoCoreException(
                $"An item event cannot be recorded for article '{article.Guid}' because it is not event-tracked.",
                FargoErrorType.InvalidOperation);
        }

        ArticleGuid = article.Guid;
        Article = article;

        EventType = eventType;

        if (occuredAt is { } instant)
        {
            OccuredAt = instant;
        }
    }

    /// <inheritdoc/>
    public EntityType GetEntityType() => EntityType.ItemEvent;

    /// <summary>
    /// Creates a new movement event for an event-tracked article.
    /// </summary>
    /// <param name="article">The event-tracked article being moved.</param>
    /// <param name="fromContainer">
    /// The origin container, or <see langword="null"/> when the quantity entered
    /// stock from outside any container.
    /// </param>
    /// <param name="toContainer">
    /// The destination container, or <see langword="null"/> when the quantity
    /// left stock to outside any container.
    /// </param>
    /// <param name="quantity">The strictly positive quantity moved.</param>
    /// <param name="occuredAt">
    /// The instant, in UTC, at which the movement occurred. When
    /// <see langword="null"/>, the current UTC instant is used.
    /// </param>
    /// <returns>
    /// A new <see cref="ItemEvent"/> of type <see cref="ItemEventType.Moviment"/>
    /// with its <see cref="Moviment"/> payload populated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the article is not event-tracked, when
    /// <paramref name="quantity"/> is not strictly positive, when the origin and
    /// destination are the same container, when the movement has neither an origin
    /// nor a destination container, or when a provided container is not a
    /// container-type article.
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
    /// Creates a new creation event for an event-tracked article, recording that
    /// a quantity of stock was created, optionally inside a container.
    /// </summary>
    /// <param name="article">The event-tracked article the stock belongs to.</param>
    /// <param name="container">
    /// The container the quantity was created in, or <see langword="null"/> when
    /// it was created outside any container.
    /// </param>
    /// <param name="quantity">The strictly positive quantity created.</param>
    /// <param name="occuredAt">
    /// The instant, in UTC, at which the creation occurred. When
    /// <see langword="null"/>, the current UTC instant is used.
    /// </param>
    /// <returns>
    /// A new <see cref="ItemEvent"/> of type <see cref="ItemEventType.Creation"/>
    /// with its <see cref="Lifecycle"/> payload populated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the article is not event-tracked, when
    /// <paramref name="quantity"/> is not strictly positive, or when
    /// <paramref name="container"/> is not a container-type article.
    /// </exception>
    public static ItemEvent NewCreationEvent(
        Article article,
        Item? container,
        Scalar quantity,
        DateTimeOffset? occuredAt = null)
        => NewLifecycleEvent(article, ItemEventType.Creation, container, quantity, occuredAt);

    /// <summary>
    /// Creates a new deletion event for an event-tracked article, recording that
    /// a quantity of stock was deleted, optionally from a container.
    /// </summary>
    /// <param name="article">The event-tracked article the stock belongs to.</param>
    /// <param name="container">
    /// The container the quantity was deleted from, or <see langword="null"/>
    /// when it was deleted from outside any container.
    /// </param>
    /// <param name="quantity">The strictly positive quantity deleted.</param>
    /// <param name="occuredAt">
    /// The instant, in UTC, at which the deletion occurred. When
    /// <see langword="null"/>, the current UTC instant is used.
    /// </param>
    /// <returns>
    /// A new <see cref="ItemEvent"/> of type <see cref="ItemEventType.Deletion"/>
    /// with its <see cref="Lifecycle"/> payload populated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the article is not event-tracked, when
    /// <paramref name="quantity"/> is not strictly positive, or when
    /// <paramref name="container"/> is not a container-type article.
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
