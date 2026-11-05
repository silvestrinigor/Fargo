using Fargo.Core.Informations;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <summary>
/// The data required to create a supply order.
/// </summary>
/// <param name="Reference">The human-facing reference for the order.</param>
/// <param name="Description">The optional description of the order.</param>
/// <param name="ReceivingContainerGuid">
/// The optional container into which goods are received. When null, goods are
/// received at the root (outside any container). The referenced item must be a
/// container accessible to the caller.
/// </param>
/// <param name="Lines">The optional initial lines of the order.</param>
/// <param name="PartitionsToAdd">
/// The optional identifiers of partitions to associate with the order.
/// </param>
public sealed record SupplyOrderCreateDto(
    Name Reference,
    Description? Description = null,
    Guid? ReceivingContainerGuid = null,
    IReadOnlyCollection<SupplyOrderLineCreateDto>? Lines = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
