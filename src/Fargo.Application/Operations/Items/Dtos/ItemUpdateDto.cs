namespace Fargo.Application.Operations.Items.Dtos;

public sealed record ItemUpdateDto(
    Guid? ParentItemContainerGuid = null,
    bool RemoveFromParentItemContainer = false,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
