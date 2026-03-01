using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.ArticleServices
{
    public class ArticleGetService(
            IArticleRepository articleRepository
            )
    {
        public async Task<Article?> GetArticle(
                User actor,
                Guid articleGuid,
                CancellationToken cancellationToken = default
                )
        {
            var article = await articleRepository.GetByGuid(
                    articleGuid,
                    [.. actor.PartitionsAccesses.Select(p => p.Guid)],
                    cancellationToken
                    );

            return article;
        }
    }
}