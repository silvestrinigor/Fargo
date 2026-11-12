using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;

namespace Fargo.Application.Operations.Articles;

/// <summary>
/// Command to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand;

/// <summary>
/// Handles commands that delete articles.
/// </summary>
/// <param name="articleService">Provides article domain operations and validation.</param>
/// <param name="actorService">Resolves the current actor and its permissions.</param>
/// <param name="articleRepository">Provides access to article entities.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class ArticleDeleteCommandHandler(
    ArticleService articleService,
    ActorResolver actorService,
    IArticleRepository articleRepository,
    ICurrentActor currentActor
    ) : ICommandHandler<ArticleDeleteCommand>
{
    /// <summary>
    /// Deletes an article after validating the current actor's permissions and access,
    /// and validating that the article can be deleted.
    /// </summary>
    /// <param name="command">The command containing the identifier of the article to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    /// <exception cref="EntityNotFoundFargoApplicationException">
    /// Thrown when the specified article cannot be found.
    /// </exception>
    public async Task HandleAsync(ArticleDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        await articleService.ValidateArticleCanBeDeletedAsync(article, cancellationToken);

        articleRepository.Remove(article);
    }
}
