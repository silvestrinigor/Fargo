using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Users.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Credentials;


namespace Fargo.Application.Operations.Users;

/// <summary>
/// Command used to update an existing user from an API update payload.
/// </summary>
public sealed record UserUpdateCommand(
    Guid UserGuid, UserUpdateDto Update
) : ICommand;

public sealed class UserUpdateCommandHandler(
    UserService userService,
    ActorResolver actorService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserUpdateCommand>
{
    public async Task HandleAsync(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        var userActor = user.Actor;

        if (command.Update.Nameid is not null)
        {
            await userService.ValidateUserNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            userActor.Nameid = command.Update.Nameid.Value;
        }

        if (command.Update.FirstName is { } firstName)
        {
            user.FirstName = firstName;
        }

        if (command.Update.LastName is { } lastName)
        {
            user.LastName = lastName;
        }

        if (command.Update.Description is { } description)
        {
            userActor.Description = description;
        }

        if (command.Update.IsActive is { } isActive)
        {
            if (isActive is true)
            {
                userActor.Activate();
            }

            else if (isActive is false)
            {
                userActor.Deactivate();
            }
        }

        if (command.Update.Authentication is { } auth)
        {
            if (auth.Password is { } password)
            {
                actor.ThrowIfPermissionDenied(ActionType.ChangeAnotherUserPassword);

                var passwordHash = passwordHasher.Hash(password);

                user.Authentication.SetPasswordHash(passwordHash);

                user.Authentication.MarkPasswordChangeAsRequired();
            }

            if (auth.DefaultPasswordExpirationPeriod is not null)
            {
                user.Authentication.DefaultPasswordExpirationPeriod = auth.DefaultPasswordExpirationPeriod;
            }
            else if (auth.RemoveDefaultPasswordExpirationPeriod is true)
            {
                user.Authentication.DefaultPasswordExpirationPeriod = null;
            }
        }

        if (command.Update.PermissionsToAdd is { Count: > 0 } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                user.Actor.AddPermission(permission);
            }
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                user.Actor.RemovePermission(permission);
            }
        }

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                user.Actor.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                user.Actor.RemovePartition(partition.Guid);
            }
        }

        if (command.Update.UserGroupsToAdd is { Count: > 0 } userGroupsToAdd)
        {
            foreach (var userGroupGuid in userGroupsToAdd.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid);

                actor.ThrowIfAccessDenied(userGroup);

                user.AddUserGroup(userGroup);
            }
        }

        if (command.Update.UserGroupsToRemove is { Count: > 0 } userGroupsToRemove)
        {
            foreach (var userGroupGuid in userGroupsToRemove.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid);

                actor.ThrowIfAccessDenied(userGroup);

                user.RemoveUserGroup(userGroup.Guid);
            }
        }

        if (command.Update.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                user.Actor.AddPartitionAccess(partition);
            }
        }

        if (command.Update.PartitionAccessesToRemove is { Count: > 0 } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                user.Actor.RemovePartitionAccess(partition.Guid);
            }
        }
    }
}
