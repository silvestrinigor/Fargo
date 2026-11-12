using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using UnitsNet;

namespace Fargo.Core.Events;

/// <summary>
/// Payload of a creation or deletion event: a quantity of an event-tracked article that entered or
/// left stock, optionally at a container. It has no per-unit identity, so one event can create or
/// delete many units.
/// </summary>
public class ItemLifecycleEvent
{
    public Guid ItemEventGuid { get; internal set; }

    /// <summary>
    /// Always strictly positive. A <see cref="Scalar"/>, so fractional and bulk amounts are allowed.
    /// </summary>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// <see langword="null"/> when there was no container, or when the container was deleted later.
    /// Use <see cref="NoContainer"/> to tell the two cases apart.
    /// </summary>
    public Guid? ItemContainerGuid { get; private init; }

    public Item? ItemContainer { get; private init; }

    /// <summary>
    /// Whether the event originally had no container. Do not infer it from
    /// <see cref="ItemContainerGuid"/>, which becomes <see langword="null"/> if the container is deleted.
    /// </summary>
    public bool NoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
    private ItemLifecycleEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

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
