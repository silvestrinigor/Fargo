using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;

namespace Fargo.Application.Commands.Articles;

public sealed record ArticleDeleteCommand(Guid ArticleGuid) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    ArticleService articleService,
    ActorResolver actorService,
    IArticleRepository articleRepository,
    ICurrentActor currentActor
    ) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task HandleAsync(ArticleDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        await articleService.ValidateArticleCanBeDeletedAsync(article, cancellationToken);

        articleRepository.Remove(article);
    }
}
