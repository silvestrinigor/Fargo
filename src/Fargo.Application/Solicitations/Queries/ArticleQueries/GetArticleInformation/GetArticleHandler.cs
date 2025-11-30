using Fargo.Application.Interfaces.Solicitations.Queries;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Queries.ArticleQueries.GetArticleInformation
{
    public class GetArticleHandler(IArticleRepository articleRepository) : IQueryHandler<GetArticleQuery, Task<ArticleInformation?>>
    {
        private readonly IArticleRepository articleRepository = articleRepository;

        public async Task<ArticleInformation?> Handle(GetArticleQuery query)
        {
            var article = await articleRepository.GetByGuidAsync(query.ArticleGuid);

            if (article is null)
            {
                return null;
            }

            return new ArticleInformation(article.Guid, article.Name, article.Description, article.CreatedAt);
        }
    }
}
