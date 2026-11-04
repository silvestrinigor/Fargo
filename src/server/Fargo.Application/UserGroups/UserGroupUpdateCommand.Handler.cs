using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupUpdateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupUpdateCommand>
{
    public async Task HandleAsync(
        UserGroupUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        if (command.Update.Nameid is not null && userGroup.Nameid != command.Update.Nameid)
        {
            await userGroupService.ValidateUserGroupNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            userGroup.Nameid = command.Update.Nameid.Value;
        }

        if (command.Update.Description is { } description)
        {
            userGroup.Description = description;
        }

        if (command.Update.IsActive is true)
        {
            userGroup.Activate();
        }
        else if (command.Update.IsActive is false)
        {
            userGroup.Deactivate();
        }

        if (command.Update.ParentUserGroup is { } parentUserGroupGuid)
        {
            var parentUserGroup = await userGroupRepository.GetByGuidAsync(parentUserGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, parentUserGroupGuid);

            actor.ThrowIfAccessDenied(parentUserGroup);

            await userGroupService.ValidateParentUserGroupAssignmentHierarchyAsync(parentUserGroup, userGroup, cancellationToken);

            userGroup.SetParentUserGroup(parentUserGroup);
        }
        else if (command.Update.RemoveParentUserGroup is true)
        {
            userGroup.RemoveFromParentUserGroup();
        }

        if (command.Update.PermissionsToAdd is { Count: > 0 } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                userGroup.AddPermission(permission);
            }
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                userGroup.RemovePermission(permission);
            }
        }

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.RemovePartition(partition.Guid);
            }
        }

        if (command.Update.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartitionAccess(partition);
            }
        }

        if (command.Update.PartitionAccessesToRemove is { Count: > 0 } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userGroup.RemovePartitionAccess(partition.Guid);
            }
        }
    }
}
