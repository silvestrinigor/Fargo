using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public class ArticleReadRepository(FargoReadDbContext context) : EntityByGuidTemporalReadRepository<ArticleReadModel>(context.Articles), IArticleReadRepository;
}
