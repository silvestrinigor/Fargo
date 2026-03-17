using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services;

public class ArticleService(
        IArticleRepository articleRepository
        )
{
    public async Task DeleteArticle(
            Article article,
            User actor,
            CancellationToken cancellationToken = default)
    {
        if (UserService.HasPermission(actor, ActionType.DeleteArticle))
        {
            throw new UserNotAuthorizedFargoDomainException(actor.Guid, ActionType.DeleteArticle);
        }

        if (!actor.IsActive)
        {
            throw new UserInactiveFargoDomainException(actor.Guid);
        }

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);
    }

    /// <summary>
    /// Gets an article by its identifier if the specified actor
    /// has access to it.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article to retrieve.
    /// </param>
    /// <param name="actor">
    /// The user performing the operation. The actor must have access
    /// to at least one partition associated with the requested article.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Article"/> when it exists and the actor
    /// has access to it; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This method enforces partition-based visibility rules.
    /// An article is only returned when the requesting user has access
    /// to at least one partition associated with it.
    /// </remarks>
    public async Task<Article?> GetArticle(
            Guid articleGuid,
            User actor,
            CancellationToken cancellationToken = default
            )
    {
        var article = await articleRepository.GetByGuid(articleGuid, cancellationToken);

        if (article != null && !PartitionService.HasAccess(article, actor))
        {
            return null;
        }

        return article;
    }
}
