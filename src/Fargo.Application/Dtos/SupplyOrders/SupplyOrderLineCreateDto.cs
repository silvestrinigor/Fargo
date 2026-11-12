using UnitsNet;

namespace Fargo.Application.Dtos.SupplyOrders;

public sealed record SupplyOrderLineCreateDto(
    Guid ArticleGuid,
    Scalar OrderedQuantity
);
