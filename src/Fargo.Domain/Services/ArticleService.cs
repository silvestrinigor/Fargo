using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ArticleService(
            IArticleRepository articleRepository
            )
    {
        public async Task ValidateArticleDelete(
                Article article,
                CancellationToken cancellationToken = default
                )
        {
            var hasItens = await articleRepository.HasItemsAssociated(
                    article.Guid,
                    cancellationToken
                    );

            if (hasItens)
            {
                throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article);
            }

            articleRepository.Remove(article);
        }
    }
}