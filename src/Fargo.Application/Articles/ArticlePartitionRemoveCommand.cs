using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to remove a partition from an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to remove.</param>
public sealed record ArticleRemovePartitionCommand(
        Guid ArticleGuid,
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleRemovePartitionCommand"/> requests.
/// </summary>
public sealed class ArticleRemovePartitionCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ArticleRemovePartitionCommand>
{
    public async Task Handle(
            ArticleRemovePartitionCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var partitionToRemove = article.Partitions.FirstOrDefault(p => p.Guid == command.PartitionGuid);

        if (partitionToRemove is not null)
        {
            actor.ValidateHasPartitionAccess(partitionToRemove.Guid);

            article.Partitions.Remove(partitionToRemove);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
