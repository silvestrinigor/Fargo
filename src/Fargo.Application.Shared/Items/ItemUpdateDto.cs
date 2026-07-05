namespace Fargo.Application.Shared.Items;

public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null,
    bool? IsActive = null);
