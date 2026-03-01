using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.ArticleServices
{
    public class ArticleDeteleService(
            IArticleRepository articleRepository
            )
    {
        public async Task DeleteArticle(
                User actor,
                Article article,
                CancellationToken cancellationToken = default
                )
        {
            actor.ValidatePermission(ActionType.DeleteArticle);

            var hasItens = await articleRepository.HasItemsAssociated(
                    article.Guid,
                    cancellationToken
                    );

            if (hasItens)
            {
                throw new ArticleDeleteWithItemsAssociatedException(article);
            }

            articleRepository.Remove(article);
        }
    }
}