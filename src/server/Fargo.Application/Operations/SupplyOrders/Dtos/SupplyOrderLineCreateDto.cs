using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <summary>
/// The data required to add a line when creating a supply order.
/// </summary>
/// <param name="ArticleGuid">The article to order.</param>
/// <param name="OrderedQuantity">The strictly positive quantity to order.</param>
public sealed record SupplyOrderLineCreateDto(
    Guid ArticleGuid,
    Scalar OrderedQuantity
);
