using Fargo.Core.Articles;

namespace Fargo.Application.Articles;

public static class ArticleRepositoryFoundEntityExtensions
{
    extension(IArticleRepository repository)
    {
        /// <summary>
        /// Retrieves an article by identifier or throws an exception when not found.
        /// </summary>
        /// <param name="articleGuid">
        /// Article unique identifier.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token.
        /// </param>
        /// <returns>
        /// Found article entity.
        /// </returns>
        /// <exception cref="ArticleNotFoundFargoApplicationException">
        /// Thrown when the article does not exist.
        /// </exception>
        public async Task<Article> GetFoundByGuid(
            Guid articleGuid,
            CancellationToken cancellationToken = default
        )
        {
            var article = await repository.GetByGuid(articleGuid, cancellationToken)
                ?? throw new ArticleNotFoundFargoApplicationException(articleGuid);

            return article;
        }
    }
}
