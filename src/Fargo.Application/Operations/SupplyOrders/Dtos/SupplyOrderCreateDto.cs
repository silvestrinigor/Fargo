using Fargo.Core.Informations;

namespace Fargo.Application.Operations.SupplyOrders.Dtos;

/// <param name="ReceivingContainerGuid">
/// <see langword="null"/> receives goods at the root. Otherwise it must be a container accessible to
/// the caller.
/// </param>
public sealed record SupplyOrderCreateDto(
    Name Reference,
    Description? Description = null,
    Guid? ReceivingContainerGuid = null,
    IReadOnlyCollection<SupplyOrderLineCreateDto>? Lines = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
