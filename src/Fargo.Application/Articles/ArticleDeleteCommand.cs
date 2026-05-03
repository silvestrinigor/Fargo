using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Events;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to delete an existing article.
/// </summary>
public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
    ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleDeleteCommand"/>.
/// </summary>
public sealed class ArticleDeleteCommandHandler(
    ActorService actorService,
    ArticleService articleService,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IEventRecorder eventRecorder,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        await articleService.DeleteArticle(article, cancellationToken);

        await eventRecorder.Record(EventType.ArticleDeleted, EntityType.Article, article.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleDeleted(article.Guid, cancellationToken);
    }
}
