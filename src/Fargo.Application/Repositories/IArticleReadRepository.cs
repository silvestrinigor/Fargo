using Fargo.Application.Models.ArticleModels;

namespace Fargo.Application.Repositories
{
    public interface IArticleReadRepository : IEntityByGuidTemporalReadRepository<ArticleReadModel>;
}
