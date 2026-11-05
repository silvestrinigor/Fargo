using Fargo.Core.Entities.SupplyOrders;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

public static class SupplyOrderDtoMappings
{
    public static readonly Expression<Func<SupplyOrder, SupplyOrderDto>> Projection = order => new SupplyOrderDto(
        order.Guid,
        order.Reference,
        order.Description,
        order.Status,
        order.CreatedAt,
        order.PlacedAt,
        order.ReceivingContainerGuid,
        order.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        order.Lines
            .Select(line => new SupplyOrderLineDto(
                line.ArticleGuid,
                line.OrderedQuantity,
                line.ReceivedQuantity,
                line.OutstandingQuantity,
                line.IsFullyReceived,
                line.IsOverReceived))
            .ToArray()
    );
}
