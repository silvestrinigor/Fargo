using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

public sealed record SupplyOrderLineCreateDto(
    Guid ArticleGuid,
    Scalar OrderedQuantity
);
