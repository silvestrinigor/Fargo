using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
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

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
