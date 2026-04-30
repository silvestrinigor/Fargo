using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to remove the image from an existing article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article whose image should be removed.</param>
public sealed record ArticleImageDeleteCommand(
        Guid ArticleGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="ArticleImageDeleteCommand"/>.
/// </summary>
public sealed class ArticleImageDeleteCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IArticleImageStorage imageStorage,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ArticleImageDeleteCommand>
{
    /// <summary>
    /// Executes the command to remove the article's image.
    /// </summary>
    /// <param name="command">The command containing the article identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    public async Task Handle(
            ArticleImageDeleteCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        if (article.Images.ImageKey is null)
        {
            return;
        }

        var imageKey = article.Images.ImageKey;

        article.Images.ImageKey = null;

        await unitOfWork.SaveChanges(cancellationToken);

        await imageStorage.DeleteAsync(imageKey, cancellationToken);
    }
}
