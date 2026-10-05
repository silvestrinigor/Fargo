namespace Fargo.Application.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    Guid? ParentContainerGuid,
    bool IsFixed,
    IReadOnlyCollection<Guid> Partitions
);
