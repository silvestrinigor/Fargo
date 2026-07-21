using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleDeleteCommandHandler(
    ArticleService articleService, ActorService actorService,
    IArticleRepository articleRepository, ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleDeleteCommandHandler> logger) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task HandleAsync(
        ArticleDeleteCommand command, CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.ArticleGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.DeleteArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        await articleService.AssertArticleCanBeDeletedAsync(article, cancellationToken);

        articleRepository.Remove(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(article.Guid, currentActor.ActorId);
    }
}
