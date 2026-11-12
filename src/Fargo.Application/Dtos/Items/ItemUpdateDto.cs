namespace Fargo.Application.Dtos.Items;

public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
