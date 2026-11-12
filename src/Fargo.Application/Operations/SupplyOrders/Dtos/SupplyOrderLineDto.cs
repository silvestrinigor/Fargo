using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <param name="OutstandingQuantity">Clamped at zero.</param>
/// <param name="IsFullyReceived">Also true when more than the ordered quantity was received.</param>
/// <param name="IsOverReceived">More than the ordered quantity was received.</param>
public sealed record SupplyOrderLineDto(
    Guid ArticleGuid,
    Scalar OrderedQuantity,
    Scalar ReceivedQuantity,
    Scalar OutstandingQuantity,
    bool IsFullyReceived,
    bool IsOverReceived
);
