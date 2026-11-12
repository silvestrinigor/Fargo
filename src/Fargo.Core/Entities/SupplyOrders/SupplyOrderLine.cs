using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.SupplyOrders;

/// <summary>
/// One line of a <see cref="SupplyOrder"/>: an article, the quantity ordered and the running total
/// received against it.
/// </summary>
/// <remarks>
/// Over-receipt is permitted: the surplus is recorded as-is (the stock physically exists) and
/// <see cref="IsOverReceived"/> flags it for review. <see cref="OutstandingQuantity"/> is clamped at
/// zero in that case.
/// </remarks>
public sealed class SupplyOrderLine
{
    public Guid SupplyOrderGuid { get; private init; }

    public SupplyOrder SupplyOrder { get; private init; } = null!;

    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; } = null!;

    /// <summary>
    /// Always strictly positive.
    /// </summary>
    public Scalar OrderedQuantity { get; private set; }

    /// <summary>
    /// May exceed <see cref="OrderedQuantity"/>.
    /// </summary>
    public Scalar ReceivedQuantity { get; private set; }

    /// <summary>
    /// Ordered minus received, clamped at zero.
    /// </summary>
    public Scalar OutstandingQuantity =>
        ReceivedQuantity >= OrderedQuantity ? 0.Amount() : OrderedQuantity - ReceivedQuantity;

    /// <summary>
    /// Whether the full ordered quantity, or more, has been received.
    /// </summary>
    public bool IsFullyReceived => ReceivedQuantity >= OrderedQuantity;

    public bool IsOverReceived => ReceivedQuantity > OrderedQuantity;

    // Required by EF Core.
    private SupplyOrderLine() { }

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

    internal void SetOrderedQuantity(Scalar orderedQuantity)
    {
        ValidateOrderedQuantity(orderedQuantity);

        OrderedQuantity = orderedQuantity;
    }

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
