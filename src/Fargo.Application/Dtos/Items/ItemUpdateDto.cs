namespace Fargo.Application.Dtos.Items;

public sealed record ItemUpdateDto(
    Guid? ParentItemContainerGuid = null,
    bool RemoveFromParentItemContainer = false,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
