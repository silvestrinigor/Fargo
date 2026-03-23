using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ArticleCommands;

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
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
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

        actor.ValidateHassPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHassAccess(article);

        await articleService.DeleteArticle(article, cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
