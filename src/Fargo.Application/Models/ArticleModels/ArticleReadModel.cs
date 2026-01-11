namespace Fargo.Application.Models.ArticleModels
{
    public record class ArticleReadModel(
        Guid Guid,
        string Name,
        string Description,
        bool IsContainer
        ) : IEntityByGuidReadModel, IEntityTemporalReadModel;
}
