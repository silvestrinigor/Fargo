using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Entities;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserUpdateCommandHandler(
    UserService userService,
    ActorResolver actorService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
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
        logger.UpdateStarted(command.UserGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        var userAudit = AuditLog.CreateAuditLog(actor, user.Guid, EntityType.User, ActionType.EditUser);

        if (command.Update.Nameid is not null)
        {
            await userService.ValidateUserNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            user.Nameid = command.Update.Nameid.Value;

            userAudit.Metadata.Add(nameof(user.Nameid), new AuditValue.String(user.Nameid));
        }

        if (command.Update.FirstName is { } firstName)
        {
            user.FirstName = firstName;

            userAudit.Metadata.Add(nameof(user.FirstName), new AuditValue.String(user.FirstName));
        }

        if (command.Update.LastName is { } lastName)
        {
            user.LastName = lastName;

            userAudit.Metadata.Add(nameof(user.LastName), new AuditValue.String(user.LastName));
        }

        if (command.Update.Description is { } description)
        {
            user.Description = description;

            userAudit.Metadata.Add(nameof(user.Description), new AuditValue.String(user.Description));
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

            userAudit.Metadata.Add(nameof(user.IsActive), new AuditValue.Boolean(user.IsActive));
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
            var auditPermissions = new List<AuditValue>();

            foreach (var permission in permissionsToAdd.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                user.AddPermission(permission);

                auditPermissions.Add(new AuditValue.Number((int)permission));
            }

            userAudit.Metadata.Add("PermissionsAdded", new AuditValue.Array(auditPermissions));
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                user.RemovePermission(permission);
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

                user.RemovePartition(partition.Guid);
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

                user.RemoveUserGroup(userGroup.Guid);
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

                user.RemovePartitionAccess(partition.Guid);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(command.UserGuid, currentActor.Guid);
    }
}
