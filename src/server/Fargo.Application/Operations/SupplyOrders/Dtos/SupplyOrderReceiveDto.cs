using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <summary>
/// One received line in a receive operation against a supply order.
/// </summary>
/// <param name="ArticleGuid">The article received (must be a line on the order).</param>
/// <param name="Quantity">The strictly positive quantity received.</param>
public sealed record SupplyOrderReceiveLineDto(
    Guid ArticleGuid,
    Scalar Quantity
);

/// <summary>
/// The data for receiving goods against a supply order. Goods are placed into
/// the order's receiving container (or at the root when the order has none).
/// </summary>
/// <param name="Lines">The received lines.</param>
public sealed record SupplyOrderReceiveDto(
    IReadOnlyCollection<SupplyOrderReceiveLineDto> Lines
);
