using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    /// <summary>
    /// Provides domain-level operations related to <see cref="Article"/>.
    /// </summary>
    public class ArticleService(
            IArticleRepository articleRepository
            )
    {
        /// <summary>
        /// Validates whether an article can be deleted.
        /// </summary>
        /// <param name="article">The article to validate.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <exception cref="ArticleDeleteWithItemsAssociatedFargoDomainException">
        /// Thrown when the article has associated items.
        /// </exception>
        public async Task ValidateArticleDelete(
                Article article,
                CancellationToken cancellationToken = default
                )
        {
            var hasItems = await articleRepository.HasItemsAssociated(
                    article.Guid,
                    cancellationToken
                    );

            if (hasItems)
            {
                throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
            }
        }
    }
}