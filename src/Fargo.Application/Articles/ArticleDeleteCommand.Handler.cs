using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles commands that delete articles.
/// </summary>
/// <param name="articleService">Provides article domain operations and validation.</param>
/// <param name="actorService">Resolves the current actor and its permissions.</param>
/// <param name="articleRepository">Provides access to article entities.</param>
/// <param name="auditLogRepository">Persists audit logs for article operations.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="unitOfWork">Coordinates persistence of the operation.</param>
/// <param name="logger">Logs the execution of the command.</param>
public sealed class ArticleDeleteCommandHandler(
    ArticleService articleService,
    ActorResolver actorService,
    IArticleRepository articleRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleDeleteCommandHandler> logger
    ) : ICommandHandler<ArticleDeleteCommand>
{
    /// <summary>
    /// Deletes an article after validating the current actor's permissions and access,
    /// validating that the article can be deleted, and recording the operation in the
    /// audit log.
    /// </summary>
    /// <param name="command">The command containing the identifier of the article to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    /// <exception cref="EntityNotFoundFargoApplicationException">
    /// Thrown when the specified article cannot be found.
    /// </exception>
    public async Task HandleAsync(
        ArticleDeleteCommand command, CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.ArticleGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.DeleteArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        await articleService.ValidateArticleCanBeDeletedAsync(article, cancellationToken);

        articleRepository.Remove(article);

        var audit = AuditLog.CreateAuditLog(actor, article, ActionType.DeleteArticle);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(article.Guid, currentActor.Guid);
    }
}
