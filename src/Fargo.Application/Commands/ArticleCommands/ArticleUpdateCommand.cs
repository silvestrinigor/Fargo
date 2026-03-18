using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

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
        PartitionService partitionService,
        IArticleRepository articleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
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
        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        UserPermissionHelper.ValidateHasPermission(actor, ActionType.EditArticle);

        var article = await articleRepository.GetByGuid(command.ArticleGuid, cancellationToken)
            ?? throw new ArticleNotFoundFargoApplicationException(command.ArticleGuid);

        var hasAccessToArticle = await partitionService.HasAccess(
            article,
            actor,
            cancellationToken
            );

        if (!hasAccessToArticle)
        {
            throw new PartitionedEntityAccessDeniedFargoApplicationException(article.Guid, actor.Guid);
        }

        if (command.Article.Name is not null)
        {
            article.Name = command.Article.Name.Value;
        }

        if (command.Article.Description is not null)
        {
            article.Description = command.Article.Description.Value;
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
