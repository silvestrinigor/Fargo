using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.ArticleRepositories;

namespace Fargo.Infrastructure.Persistence.Repositories.ArticleRepositories
{
    public class ArticleReadRepository(FargoContext context) : EntityByGuidTemporalReadRepository<Article>(context.Articles), IArticleReadRepository;
}
