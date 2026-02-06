namespace Fargo.Application.Models.ItemModels
{
    public record ItemResponseModel(
            Guid Guid,
            Guid ArticleGuid,
            Guid? ParentItemGuid
            );
}