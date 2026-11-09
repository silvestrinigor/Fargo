using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Informations;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <summary>
/// Read model for a supply order.
/// </summary>
/// <param name="Guid">The unique identifier of the order.</param>
/// <param name="Reference">The human-facing reference for the order.</param>
/// <param name="Description">The description of the order.</param>
/// <param name="Status">The current lifecycle state.</param>
/// <param name="CreatedAt">When the order was created, in UTC.</param>
/// <param name="PlacedAt">When the order was placed, in UTC, or null while a draft.</param>
/// <param name="ReceivingContainerGuid">
/// The container into which goods are received, or null to receive at the root.
/// </param>
/// <param name="Partitions">The identifiers of the partitions associated with the order.</param>
/// <param name="Lines">The lines of the order.</param>
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
