using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// Defines one line of a <see cref="SupplyOrder"/>: an article and the quantity
/// ordered, together with the running total already received against it.
/// </summary>
/// <remarks>
/// Over-receipt is permitted: <see cref="ReceivedQuantity"/> may exceed
/// <see cref="OrderedQuantity"/> when more than was ordered is delivered. The
/// surplus is recorded as-is (the stock physically exists), and
/// <see cref="IsOverReceived"/> flags the line so the anomaly is visible for
/// review. <see cref="OutstandingQuantity"/> is clamped at zero in that case.
/// </remarks>
public sealed class SupplyOrderLine
{
    /// <summary>
    /// Gets the unique identifier of the supply order that owns this line.
    /// </summary>
    public Guid SupplyOrderGuid { get; private init; }

    /// <summary>
    /// Gets the supply order that owns this line.
    /// </summary>
    public SupplyOrder SupplyOrder { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the ordered article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the ordered article.
    /// </summary>
    public Article Article { get; private init; } = null!;

    /// <summary>
    /// Gets the quantity ordered. Always strictly positive.
    /// </summary>
    public Scalar OrderedQuantity { get; private set; }

    /// <summary>
    /// Gets the quantity received so far. May exceed <see cref="OrderedQuantity"/>
    /// when more than was ordered is delivered.
    /// </summary>
    public Scalar ReceivedQuantity { get; private set; }

    /// <summary>
    /// Gets the still-outstanding quantity (ordered minus received), clamped at
    /// zero once the line is fully or over-received.
    /// </summary>
    public Scalar OutstandingQuantity =>
        ReceivedQuantity >= OrderedQuantity ? 0.Amount() : OrderedQuantity - ReceivedQuantity;

    /// <summary>
    /// Gets a value indicating whether the full ordered quantity (or more) has
    /// been received.
    /// </summary>
    public bool IsFullyReceived => ReceivedQuantity >= OrderedQuantity;

    /// <summary>
    /// Gets a value indicating whether more than the ordered quantity has been
    /// received. Flags an over-receipt for review; the surplus is still recorded.
    /// </summary>
    public bool IsOverReceived => ReceivedQuantity > OrderedQuantity;

    /// <summary>
    /// Initializes a new <see cref="SupplyOrderLine"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private SupplyOrderLine() { }

    /// <summary>
    /// Initializes a new supply order line for the specified article and ordered
    /// quantity. Received quantity starts at zero.
    /// </summary>
    /// <param name="supplyOrder">The owning supply order.</param>
    /// <param name="article">The ordered article.</param>
    /// <param name="orderedQuantity">The strictly positive ordered quantity.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when <paramref name="orderedQuantity"/> is not strictly positive.
    /// </exception>
    internal SupplyOrderLine(SupplyOrder supplyOrder, Article article, Scalar orderedQuantity)
    {
        ArgumentNullException.ThrowIfNull(supplyOrder);
        ArgumentNullException.ThrowIfNull(article);

        ValidateOrderedQuantity(orderedQuantity);

        SupplyOrder = supplyOrder;
        SupplyOrderGuid = supplyOrder.Guid;

        Article = article;
        ArticleGuid = article.Guid;

        OrderedQuantity = orderedQuantity;
        ReceivedQuantity = 0.Amount();
    }

    /// <summary>
    /// Changes the ordered quantity of this line.
    /// </summary>
    /// <param name="orderedQuantity">The new strictly positive ordered quantity.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when <paramref name="orderedQuantity"/> is not strictly positive.
    /// </exception>
    internal void SetOrderedQuantity(Scalar orderedQuantity)
    {
        ValidateOrderedQuantity(orderedQuantity);

        OrderedQuantity = orderedQuantity;
    }

    /// <summary>
    /// Records that <paramref name="quantity"/> was received against this line.
    /// Over-receipt is allowed: the received total may exceed the ordered total.
    /// </summary>
    /// <param name="quantity">The strictly positive quantity received.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when <paramref name="quantity"/> is not strictly positive.
    /// </exception>
    internal void AddReceived(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new FargoCoreException(
                "Received quantity must be greater than zero.",
                FargoErrorType.InvalidOperation);
        }

        ReceivedQuantity += quantity;
    }

    private static void ValidateOrderedQuantity(Scalar orderedQuantity)
    {
        if (orderedQuantity <= 0.Amount())
        {
            throw new FargoCoreException(
                "A supply order line quantity must be greater than zero.",
                FargoErrorType.InvalidOperation);
        }
    }
}
