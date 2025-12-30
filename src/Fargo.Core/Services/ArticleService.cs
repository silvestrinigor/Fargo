using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository articleRepository) : IArticleService
    {
        private readonly IArticleRepository articleRepository = articleRepository;

        internal Article CreateNewEntity(Article entity)
        {
            throw new NotImplementedException();
        }
    }
}
