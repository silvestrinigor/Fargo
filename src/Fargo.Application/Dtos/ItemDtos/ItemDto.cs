namespace Fargo.Application.Dtos.ItemDtos
{
    public record ItemDto(
        Guid Guid,
        Guid ArticleGuid,
        Guid? ParentItemGuid
        );
}
