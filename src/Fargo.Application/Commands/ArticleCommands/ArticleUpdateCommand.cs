using Fargo.Application.Events;
using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Commands.ArticleCommands;

/// <summary>
/// Command used to update an existing article.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article to update.
/// </param>
/// <param name="Article">
/// The data used to update the article.
/// </param>
public sealed record ArticleUpdateCommand(
        Guid ArticleGuid,
        ArticleUpdateModel Article
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="ArticleUpdateCommand"/>.
/// </summary>
public sealed class ArticleUpdateCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<ArticleUpdateCommand>
{
    /// <summary>
    /// Executes the command to update an existing article.
    /// </summary>
    /// <param name="command">The command containing the article identifier and update data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    public async Task Handle(
            ArticleUpdateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        if (command.Article.Name is not null)
        {
            article.Name = command.Article.Name.Value;
        }

        if (command.Article.Description is not null)
        {
            article.Description = command.Article.Description.Value;
        }

        if (command.Article.Mass is not null)
        {
            article.Mass = command.Article.Mass;
        }

        if (command.Article.LengthX is not null)
        {
            article.LengthX = command.Article.LengthX;
        }

        if (command.Article.LengthY is not null)
        {
            article.LengthY = command.Article.LengthY;
        }

        if (command.Article.LengthZ is not null)
        {
            article.LengthZ = command.Article.LengthZ;
        }

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleUpdated(article.Guid, cancellationToken);
    }
}
