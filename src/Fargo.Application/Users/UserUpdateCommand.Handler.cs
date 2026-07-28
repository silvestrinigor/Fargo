using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserUpdateCommandHandler(
    UserService userService,
    ActorService actorService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserUpdateCommandHandler> logger
) : ICommandHandler<UserUpdateCommand>
{
    public async Task HandleAsync(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.UserGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.EditUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        if (command.Update.Nameid is not null)
        {
            await userService.ValidateUserNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            user.Nameid = command.Update.Nameid.Value;
        }

        user.FirstName = command.Update.FirstName ?? user.FirstName;

        user.LastName = command.Update.LastName ?? user.LastName;

        user.Description = command.Update.Description ?? user.Description;

        user.IsActive = command.Update.IsActive ?? user.IsActive;

        if (command.Update.PermissionsToAdd is { Count: > 0 } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission.Action);

                user.AddPermission(permission.Action);
            }
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission.Action);

                user.RemovePermission(permission.Action);
            }
        }

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.RemovePartition(partition);
            }
        }

        if (command.Update.UserGroupsToAdd is { Count: > 0 } userGroupsToAdd)
        {
            foreach (var userGroupGuid in userGroupsToAdd.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid, EntityType.UserGroup);

                actor.ThrowIfAccessDenied(userGroup);

                user.AddUserGroup(userGroup);
            }
        }

        if (command.Update.UserGroupsToRemove is { Count: > 0 } userGroupsToRemove)
        {
            foreach (var userGroupGuid in userGroupsToRemove.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid, EntityType.UserGroup);

                actor.ThrowIfAccessDenied(userGroup);

                user.RemoveUserGroup(userGroup);
            }
        }

        if (command.Update.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.AddPartitionAccess(partition);
            }
        }

        if (command.Update.PartitionAccessesToRemove is { Count: > 0 } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.RemovePartitionAccess(partition);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(command.UserGuid, currentActor.ActorId);
    }
}
