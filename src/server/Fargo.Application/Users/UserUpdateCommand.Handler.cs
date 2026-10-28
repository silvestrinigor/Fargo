using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Application.Users;

public sealed class UserUpdateCommandHandler(
    UserService userService,
    ActorResolver actorService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    IAuditLogRepository auditLogRepository,
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

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        var userAudit = AuditLog.CreateAuditLog(actor, user, AuditLogType.EntityUpdated);

        if (command.Update.Nameid is not null)
        {
            await userService.ValidateUserNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            user.Nameid = command.Update.Nameid.Value;

            userAudit.Metadata.AddNameid(user.Nameid);
        }

        if (command.Update.FirstName is { } firstName)
        {
            user.FirstName = firstName;

            userAudit.Metadata.AddFirstName(firstName);
        }

        if (command.Update.LastName is { } lastName)
        {
            user.LastName = lastName;

            userAudit.Metadata.AddLastName(lastName);
        }

        if (command.Update.Description is { } description)
        {
            user.Description = description;

            userAudit.Metadata.AddDescription(user.Description);
        }

        if (command.Update.IsActive is { } isActive)
        {
            if (isActive is true)
            {
                user.Activate();
            }

            else if (isActive is false)
            {
                user.Deactivate();
            }

            userAudit.Metadata.AddIsActive(user.IsActive);
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

                user.AddPermission(permission);
            }

            userAudit.Metadata.AddPermissionsAdded([.. permissionsToAdd.Distinct()]);
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                user.RemovePermission(permission);
            }

            userAudit.Metadata.AddPermissionsRemoved([.. permissionsToRemove.Distinct()]);
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

            userAudit.Metadata.AddPartitionsAdded(partitionsToAdd);
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.RemovePartition(partition.Guid);
            }

            userAudit.Metadata.AddPartitionsRemoved(partitionsToRemove);
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

            userAudit.Metadata.AddUserGroupsAdded([.. userGroupsToAdd.Distinct()]);
        }

        if (command.Update.UserGroupsToRemove is { Count: > 0 } userGroupsToRemove)
        {
            foreach (var userGroupGuid in userGroupsToRemove.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid, EntityType.UserGroup);

                actor.ThrowIfAccessDenied(userGroup);

                user.RemoveUserGroup(userGroup.Guid);
            }

            userAudit.Metadata.AddUserGroupsRemoved([.. userGroupsToRemove.Distinct()]);
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

            userAudit.Metadata.AddPartitionAccessesAdded([.. partitionAccessesToAdd]);
        }

        if (command.Update.PartitionAccessesToRemove is { Count: > 0 } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.RemovePartitionAccess(partition.Guid);
            }

            userAudit.Metadata.AddPartitionAccessesRemoved([.. partitionAccessesToRemove]);
        }

        auditLogRepository.Add(userAudit);
    }
}
