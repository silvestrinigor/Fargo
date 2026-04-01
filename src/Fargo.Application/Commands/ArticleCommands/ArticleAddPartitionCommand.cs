using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ArticleCommands;

public sealed record ArticleAddPartitionCommand(
        Guid ArticleGuid,
        Guid PartitionGuid
        ) : ICommand;

public sealed class ArticleAddPartitionCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ArticleAddPartitionCommand>
{
    public async Task Handle(
            ArticleAddPartitionCommand command,
            CancellationToken cancellationToken = default
            )
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
