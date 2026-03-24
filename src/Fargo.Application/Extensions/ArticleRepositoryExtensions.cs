using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IArticleRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
/// <remarks>
/// These helpers encapsulate common patterns such as retrieving entities
/// and ensuring their existence, reducing duplication and improving
/// consistency across the application layer.
/// </remarks>
public static class ArticleRepositoryExtensions
{
    extension(IArticleRepository repository)
    {
        /// <summary>
        /// Retrieves an <see cref="Article"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="articleGuid">
        /// The unique identifier of the article.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="Article"/> associated with the specified GUID.
        /// </returns>
        /// <exception cref="ArticleNotFoundFargoApplicationException">
        /// Thrown when no article is found with the specified GUID.
        /// </exception>
        /// <remarks>
        /// This method enforces a fail-fast approach by throwing an exception
        /// when the requested entity does not exist, avoiding the need for
        /// null checks in calling code.
        /// </remarks>
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
