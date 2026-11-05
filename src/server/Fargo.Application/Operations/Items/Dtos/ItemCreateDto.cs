namespace Fargo.Application.Operations.Items.Dtos;

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    Guid? ParentItemContainerGuid = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
