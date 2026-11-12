using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Informations;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <param name="CreatedAt">UTC.</param>
/// <param name="PlacedAt">UTC, or <see langword="null"/> while a draft.</param>
/// <param name="ReceivingContainerGuid"><see langword="null"/> receives goods at the root.</param>
public sealed record SupplyOrderDto(
    Guid Guid,
    Name Reference,
    Description Description,
    SupplyOrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PlacedAt,
    Guid? ReceivingContainerGuid,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<SupplyOrderLineDto> Lines
);

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
