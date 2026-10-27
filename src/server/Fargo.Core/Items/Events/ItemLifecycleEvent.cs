using Fargo.Core.Articles;
using Fargo.Core.Common;
using UnitsNet;

namespace Fargo.Core.Items.Events;

/// <summary>
/// Represents the payload of a creation or deletion event: a quantity of an
/// event-tracked article that entered (<see cref="ItemEventType.Creation"/>) or
/// left (<see cref="ItemEventType.Deletion"/>) stock, optionally at a specific
/// container.
/// </summary>
/// <remarks>
/// Event-based tracking does not materialize one <see cref="Item"/> row per
/// physical unit, so a creation or deletion carries no per-unit identity. It is
/// described only by general information: how much (<see cref="Quantity"/>) and
/// where (<see cref="ItemContainer"/>). A single event can therefore create or
/// delete many units at once.
/// </remarks>
public class ItemLifecycleEvent
{
    /// <summary>
    /// Gets the unique identifier of the owning <see cref="ItemEvent"/>, which is
    /// also the primary key of this record.
    /// </summary>
    public Guid ItemEventGuid { get; internal set; }

    /// <summary>
    /// Gets the quantity of the article that was created or deleted.
    /// </summary>
    /// <remarks>
    /// Always strictly positive. Because the quantity is a <see cref="Scalar"/>,
    /// event-based tracking supports fractional and bulk amounts, not only whole
    /// units.
    /// </remarks>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the container the quantity was created in or
    /// deleted from, if any.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> when the quantity was created or deleted outside any
    /// container. It may also become <see langword="null"/> later if the container
    /// is deleted; <see cref="NoContainer"/> preserves whether the event
    /// originally had no container.
    /// </remarks>
    public Guid? ItemContainerGuid { get; private init; }

    /// <summary>
    /// Gets the container the quantity was created in or deleted from, if any.
    /// </summary>
    public Item? ItemContainer { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the event originally had no container.
    /// </summary>
    /// <remarks>
    /// This flag must not be inferred from <see cref="ItemContainerGuid"/>,
    /// because that identifier may become <see langword="null"/> if the container
    /// is later deleted.
    /// </remarks>
    public bool NoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new lifecycle event entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ItemLifecycleEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Creates a new creation/deletion payload for an event-tracked article.
    /// </summary>
    /// <param name="article">The event-tracked article.</param>
    /// <param name="container">
    /// The container the quantity was created in or deleted from, or
    /// <see langword="null"/> when outside any container.
    /// </param>
    /// <param name="quantity">The strictly positive quantity.</param>
    /// <returns>A new <see cref="ItemLifecycleEvent"/> payload.</returns>
    /// <remarks>
    /// The returned payload is not yet attached to an <see cref="ItemEvent"/>;
    /// the owning event links it and assigns its <see cref="ItemEventGuid"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when <paramref name="quantity"/> is not strictly positive, or when
    /// <paramref name="container"/> is not a container-type article.
    /// </exception>
    internal static ItemLifecycleEvent NewItemLifecycleEvent(
        Article article,
        Item? container,
        Scalar quantity)
    {
        ArgumentNullException.ThrowIfNull(article);

        if (quantity.Amount <= 0)
        {
            throw new FargoCoreException(
                "The quantity of a creation or deletion event must be strictly positive.",
                FargoErrorType.InvalidOperation);
        }

        if (container is not null && container.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{container.Guid}' is not a container item.",
                FargoErrorType.InvalidOperation);
        }

        return new ItemLifecycleEvent
        {
            Quantity = quantity,
            ItemContainer = container,
            ItemContainerGuid = container?.Guid,
            NoContainer = container is null,
        };
    }
}
