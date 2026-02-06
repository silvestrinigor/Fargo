namespace Fargo.Application.Models.ArticleModels
{
    public record class ArticleResponseModel(
            Guid Guid,
            string Name,
            string Description,
            bool IsContainer
            );
}