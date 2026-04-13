using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Application.Storage;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ArticleCommands;

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

        // If the article already has an image, delete the old one before storing the new one.
        if (article.ImageKey is not null)
        {
            await imageStorage.DeleteAsync(article.ImageKey, cancellationToken);
        }

        article.ImageKey = await imageStorage.SaveAsync(
            command.ArticleGuid,
            command.ImageStream,
            command.ContentType,
            cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
