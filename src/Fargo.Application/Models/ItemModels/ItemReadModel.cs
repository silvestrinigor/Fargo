namespace Fargo.Application.Models.ItemModels
{
    public record ItemReadModel(
        Guid Guid,
        Guid ArticleGuid,
        Guid? ParentItemGuid = null
        ) : IEntityByGuidReadModel, IEntityTemporalReadModel;
}
