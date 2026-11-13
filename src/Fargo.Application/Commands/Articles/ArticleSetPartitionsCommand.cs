using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Articles;

public sealed record ArticleSetPartitionsCommand(Guid ArticleGuid, PartitionMembershipsChangeDto Change) : ICommand;

public sealed class ArticleSetPartitionsCommandHandler(
    ActorResolver actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ArticleSetPartitionsCommand>
{
    public async Task HandleAsync(
        ArticleSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeArticlePartitions);

        var plan = PartitionsChangePlan.FromMemberships(command.Change);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        await plan.ApplyMembershipsAsync(
            actor,
            partitionRepository,
            article.AddPartition,
            article.RemovePartition,
            cancellationToken
        );
    }
}
