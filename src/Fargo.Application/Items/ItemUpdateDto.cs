namespace Fargo.Application.Items;

public sealed record ItemUpdateDto(
    Guid? ParentItemContainerGuid = null,
    bool RemoveFromParentItemContainer = false,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
