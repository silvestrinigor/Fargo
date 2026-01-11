using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.ArticleRepositories
{
    public interface IArticleReadRepository : IEntityByGuidTemporalReadRepository<Article>;
}
