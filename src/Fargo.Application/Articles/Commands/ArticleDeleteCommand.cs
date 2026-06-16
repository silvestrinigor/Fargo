using Fargo.Application.Identity;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Commands;

/// <summary>
/// Command used to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deletion.
/// </summary>
public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    IEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);

        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {articleGuid} by actor {actorId}.",
                command.ArticleGuid,
                actor.ActorId);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken);

        if (hasItems)
        {
            logger.LogWarning(
                "Article delete flow rejected because article {articleGuid} has associated items.",
                article.Guid);

            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        entityEventRepository.Add(Event.EntityDeleted(article, actor.ActorId));

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete mutation completed for article {articleGuid} by actor {actorId}.",
                article.Guid,
                actor.ActorId);
        }
    }
}
