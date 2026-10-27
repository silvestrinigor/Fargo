using Fargo.Core.Articles;
using Fargo.Core.Common;
using UnitsNet;

namespace Fargo.Core.Items.Events;

/// <summary>
/// Represents the movement of a quantity of an event-tracked article between
/// containers (or into/out of no container).
/// </summary>
/// <remarks>
/// This is the event-based counterpart of <see cref="ItemMoviment"/>. Where an
/// entity-tracked item records a movement of a single physical unit it is an
/// instance of, an <see cref="ItemMovimentEvent"/> records a movement of an
/// arbitrary <see cref="Quantity"/> of an article for which no per-unit
/// <see cref="Item"/> rows exist.
///
/// A movement has an origin and a destination, each of which may be a container
/// item or "no container":
/// <list type="bullet">
///   <item>
///     <description>
///     A receipt into a container is a movement from no container
///     (<see cref="FromNoContainer"/>) to a destination container.
///     </description>
///   </item>
///   <item>
///     <description>
///     A dispatch out of stock is a movement from a container to no container
///     (<see cref="ToNoContainer"/>).
///     </description>
///   </item>
///   <item>
///     <description>
///     A transfer is a movement from one container to another.
///     </description>
///   </item>
/// </list>
///
/// Event-based inventory for a container is the net of signed quantities: the
/// sum of quantities moved into the container minus the sum of quantities moved
/// out of it, considering only events at or before the instant being evaluated.
/// </remarks>
public class ItemMovimentEvent
{
    /// <summary>
    /// Gets the unique identifier of the owning <see cref="ItemEvent"/>, which is
    /// also the primary key of this record.
    /// </summary>
    public Guid ItemEventGuid { get; private init; }

    /// <summary>
    /// Gets the owning base event.
    /// </summary>
    public ItemEvent ItemEvent { get; private init; }

    /// <summary>
    /// Gets the quantity of the article that was moved.
    /// </summary>
    /// <remarks>
    /// The quantity is always strictly positive. Direction is expressed by the
    /// origin and destination of the movement, not by the sign of this value.
    /// Because the quantity is a <see cref="Scalar"/>, event-based tracking
    /// supports fractional and bulk amounts, not only whole units.
    /// </remarks>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the container the quantity was moved from,
    /// if any.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> when the movement originated outside any container.
    /// It may also become <see langword="null"/> later if the origin container is
    /// deleted; <see cref="FromNoContainer"/> preserves whether the movement
    /// originally had no origin container.
    /// </remarks>
    public Guid? FromItemContainerGuid { get; private init; }

    /// <summary>
    /// Gets the container the quantity was moved from, if any.
    /// </summary>
    public Item? FromItemContainer { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the movement originally had no origin
    /// container.
    /// </summary>
    /// <remarks>
    /// This flag must not be inferred from <see cref="FromItemContainerGuid"/>,
    /// because that identifier may become <see langword="null"/> if the origin
    /// container is later deleted.
    /// </remarks>
    public bool FromNoContainer { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the container the quantity was moved to, if
    /// any.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> when the movement left the quantity outside any
    /// container. It may also become <see langword="null"/> later if the
    /// destination container is deleted; <see cref="ToNoContainer"/> preserves
    /// whether the movement originally had no destination container.
    /// </remarks>
    public Guid? ToItemContainerGuid { get; private init; }

    /// <summary>
    /// Gets the container the quantity was moved to, if any.
    /// </summary>
    public Item? ToItemContainer { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the movement originally had no destination
    /// container.
    /// </summary>
    /// <remarks>
    /// This flag must not be inferred from <see cref="ToItemContainerGuid"/>,
    /// because that identifier may become <see langword="null"/> if the
    /// destination container is later deleted.
    /// </remarks>
    public bool ToNoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new movement event entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ItemMovimentEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private ItemMovimentEvent(Article article, DateTimeOffset? occuredAt)
    {
        var itemEvent = new ItemEvent(article, ItemEventType.Moviment, occuredAt);

        ItemEvent = itemEvent;
        ItemEventGuid = itemEvent.Guid;
    }

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
    /// <returns>A new <see cref="ItemMovimentEvent"/> instance.</returns>
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
    public static ItemMovimentEvent NewItemMovimentEvent(
        Article article,
        Item? fromContainer,
        Item? toContainer,
        Scalar quantity,
        DateTimeOffset? occuredAt = null)
    {
        ArgumentNullException.ThrowIfNull(article);

        if (quantity.Amount <= 0)
        {
            throw new FargoCoreException(
                "The quantity of a movement event must be strictly positive.",
                FargoErrorType.InvalidOperation);
        }

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

        ThrowIfNotContainer(fromContainer);
        ThrowIfNotContainer(toContainer);

        var itemMovimentEvent = new ItemMovimentEvent(article, occuredAt)
        {
            Quantity = quantity,

            FromItemContainer = fromContainer,
            FromItemContainerGuid = fromContainer?.Guid,
            FromNoContainer = fromContainer is null,

            ToItemContainer = toContainer,
            ToItemContainerGuid = toContainer?.Guid,
            ToNoContainer = toContainer is null,
        };

        return itemMovimentEvent;
    }

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
