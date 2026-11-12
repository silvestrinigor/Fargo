using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using UnitsNet;

namespace Fargo.Core.Events;

/// <summary>
/// Payload of a movement event: a quantity of an event-tracked article moved between containers,
/// or into or out of no container.
/// </summary>
/// <remarks>
/// Event-based counterpart of <see cref="ItemMoviment"/>. A receipt is a movement from no container,
/// a dispatch is a movement to no container, and a transfer is container to container. The
/// inventory of a container is the quantity moved into it minus the quantity moved out of it,
/// counting only events at or before the evaluated instant.
/// </remarks>
public class ItemMovimentEvent
{
    public Guid ItemEventGuid { get; internal set; }

    /// <summary>
    /// Always strictly positive; direction comes from origin and destination, not from the sign.
    /// </summary>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// <see langword="null"/> when there was no origin container, or when it was deleted later.
    /// Use <see cref="FromNoContainer"/> to tell the two cases apart.
    /// </summary>
    public Guid? FromItemContainerGuid { get; private init; }

    public Item? FromItemContainer { get; private init; }

    /// <summary>
    /// Whether the movement originally had no origin container. Do not infer it from
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
    /// Whether the movement originally had no destination container. Do not infer it from
    /// <see cref="ToItemContainerGuid"/>, which becomes <see langword="null"/> if the container is deleted.
    /// </summary>
    public bool ToNoContainer { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Required by EF Core.
    private ItemMovimentEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal static ItemMovimentEvent NewItemMovimentEvent(
        Article article,
        Item? fromContainer,
        Item? toContainer,
        Scalar quantity)
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

        return new ItemMovimentEvent
        {
            Quantity = quantity,

            FromItemContainer = fromContainer,
            FromItemContainerGuid = fromContainer?.Guid,
            FromNoContainer = fromContainer is null,

            ToItemContainer = toContainer,
            ToItemContainerGuid = toContainer?.Guid,
            ToNoContainer = toContainer is null,
        };
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
