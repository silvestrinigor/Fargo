namespace Fargo.Application.Operations.Items.Dtos;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    Guid? ParentContainerGuid,
    bool IsFixed,
    IReadOnlyCollection<Guid> Partitions
);
