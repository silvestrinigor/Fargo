using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.ArticleCommands;

public sealed record ArticleRemovePartitionCommand(
        Guid ArticleGuid,
        Guid PartitionGuid
        ) : ICommand;

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
