using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to delete an existing article.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article to delete.
/// </param>
public sealed record ArticleDeleteCommand(
        Guid ArticleGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="ArticleDeleteCommand"/>.
/// </summary>
public sealed class ArticleDeleteCommandHandler(
        ActorService actorService,
        ArticleService articleService,
        IArticleRepository articleRepository,
        IArticleImageStorage imageStorage,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<ArticleDeleteCommand>
{
    /// <summary>
    /// Executes the command to delete an existing article.
    /// </summary>
    /// <param name="command">The command containing the article identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    public async Task Handle(
            ArticleDeleteCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        await articleService.DeleteArticle(article, cancellationToken);

        if (article.ImageKey is not null)
        {
            await imageStorage.DeleteAsync(article.ImageKey, cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleDeleted(article.Guid, cancellationToken);
    }
}
