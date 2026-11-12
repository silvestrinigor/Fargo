using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <param name="ArticleGuid">Must be a line on the order.</param>
public sealed record SupplyOrderReceiveLineDto(
    Guid ArticleGuid,
    Scalar Quantity
);

/// <summary>
/// Goods are placed into the order's receiving container, or at the root when it has none.
/// </summary>
public sealed record SupplyOrderReceiveDto(
    IReadOnlyCollection<SupplyOrderReceiveLineDto> Lines
);
