using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <summary>
/// Read model for one line of a supply order.
/// </summary>
/// <param name="ArticleGuid">The ordered article.</param>
/// <param name="OrderedQuantity">The quantity ordered.</param>
/// <param name="ReceivedQuantity">The quantity received so far.</param>
/// <param name="OutstandingQuantity">The still-outstanding quantity (clamped at zero).</param>
/// <param name="IsFullyReceived">Whether the full ordered quantity (or more) has been received.</param>
/// <param name="IsOverReceived">Whether more than the ordered quantity has been received.</param>
public sealed record SupplyOrderLineDto(
    Guid ArticleGuid,
    Scalar OrderedQuantity,
    Scalar ReceivedQuantity,
    Scalar OutstandingQuantity,
    bool IsFullyReceived,
    bool IsOverReceived
);
