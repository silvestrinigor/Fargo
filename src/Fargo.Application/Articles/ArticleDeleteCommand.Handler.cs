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
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.ArticleGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.DeleteArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(article);

        actor.ThrowIfAccessNotAuthorized(article);

        await articleService.AssertArticleCanBeDeletedAsync(article, cancellationToken);

        articleRepository.Remove(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(article.Guid, currentActor.ActorId);
    }
}
