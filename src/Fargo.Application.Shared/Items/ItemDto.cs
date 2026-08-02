namespace Fargo.Application.Shared.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    Guid? ParentContainerGuid,
    IReadOnlyCollection<Guid> Partitions);
