using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to add a partition to an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="PartitionGuid">The unique identifier of the partition to add.</param>
public sealed record ArticleAddPartitionCommand(
    Guid ArticleGuid,
    Guid PartitionGuid
    ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleAddPartitionCommand"/> requests.
/// </summary>
public sealed class ArticleAddPartitionCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
    ) : ICommandHandler<ArticleAddPartitionCommand>
{
    // TODO: document this function
    public async Task Handle(
        ArticleAddPartitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (!article.Partitions.Contains(partition))
        {
            article.Partitions.Add(partition);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
