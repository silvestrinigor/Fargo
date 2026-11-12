using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.UserGroups;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.UserGroups;

public sealed record UserGroupCreateCommand(UserGroupCreateDto Create) : ICommand<Guid>;

public sealed class UserGroupCreateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateUserGroup);

        await userGroupService.ValidateUserGroupNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var userGroup = UserGroup.CreateUserGroup(command.Create.Nameid);

        userGroup.Description = command.Create.Description ?? Description.Empty;

        if (command.Create.IsActive is true)
        {
            userGroup.Activate();
        }
        else if (command.Create.IsActive is false)
        {
            userGroup.Deactivate();
        }

        if (command.Create.ParentUserGroup is { } parentUserGroupGuid)
        {
            var parentUserGroup = await userGroupRepository.GetByGuidAsync(parentUserGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, parentUserGroupGuid);

            actor.ThrowIfAccessDenied(parentUserGroup);

            userGroup.SetParentUserGroup(parentUserGroup);
        }

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Distinct();

            foreach (var action in requestedActions)
            {
                actor.ThrowIfPermissionDenied(action);

                userGroup.AddPermission(action);
            }
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartition(partition);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartitionAccess(partition);
            }
        }

        userGroupRepository.Add(userGroup);

        return userGroup.Guid;
    }
}
