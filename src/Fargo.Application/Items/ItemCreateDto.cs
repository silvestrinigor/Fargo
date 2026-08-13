namespace Fargo.Application.Items;

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    Guid? ParentItemContainerGuid = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
