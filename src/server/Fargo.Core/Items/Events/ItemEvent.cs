using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Entities;

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
/// <see cref="ItemMovimentEvent"/>, which references the <see cref="ItemEvent"/>
/// through its primary key.
///
/// An <see cref="ItemEvent"/> can only be created for an article whose
/// <see cref="Article.TrackingMode"/> is <see cref="ArticleTrackingMode.Event"/>.
/// </remarks>
public class ItemEvent : IEntity
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
}
