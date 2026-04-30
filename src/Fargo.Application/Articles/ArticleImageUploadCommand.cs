using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to upload or replace the image of an existing article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="ImageStream">The image data to store.</param>
/// <param name="ContentType">The MIME type of the image (e.g., <c>image/jpeg</c>).</param>
public sealed record ArticleImageUploadCommand(
        Guid ArticleGuid,
        Stream ImageStream,
        string ContentType
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="ArticleImageUploadCommand"/>.
/// </summary>
public sealed class ArticleImageUploadCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IArticleImageStorage imageStorage,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ArticleImageUploadCommand>
{
    /// <summary>
    /// Executes the command to upload or replace the article's image.
    /// </summary>
    /// <param name="command">The command containing the article identifier and image data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    public async Task Handle(
            ArticleImageUploadCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var previousImageKey = article.ImageKey;
        var newImageKey = await imageStorage.SaveAsync(
            command.ArticleGuid,
            command.ImageStream,
            command.ContentType,
            cancellationToken);

        article.ImageKey = newImageKey;

        try
        {
            await unitOfWork.SaveChanges(cancellationToken);
        }
        catch
        {
            await imageStorage.DeleteAsync(newImageKey, cancellationToken);

            article.ImageKey = previousImageKey;

            throw;
        }

        if (previousImageKey is not null && previousImageKey != newImageKey)
        {
            await imageStorage.DeleteAsync(previousImageKey, cancellationToken);
        }
    }
}
