using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.UserGroups;

public sealed record UserGroupSetPartitionsCommand(Guid UserGroupGuid, PartitionsChangeDto Change) : ICommand;

public sealed class UserGroupSetPartitionsCommandHandler(
    ActorResolver actorService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupSetPartitionsCommand>
{
    public async Task HandleAsync(
        UserGroupSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserGroupPartitions);

        var plan = PartitionsChangePlan.From(command.Change);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        await plan.ApplyAsync(
            actor,
            partitionRepository,
            userGroup.AddPartition,
            userGroup.RemovePartition,
            userGroup.AddPartitionAccess,
            userGroup.RemovePartitionAccess,
            cancellationToken);
    }
}
