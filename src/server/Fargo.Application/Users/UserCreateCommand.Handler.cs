using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Application.Users;

public sealed class UserCreateCommandHandler(
    ActorResolver actorService,
    UserService userService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IPasswordHasher passwordHasher
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateUser);

        await userService.ValidateUserNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var user = Actor.NewUser(command.Create.Nameid).User!;
        var userActor = user.Actor;

        var userAudit = AuditLog.CreateAuditLog(actor, userActor, AuditLogType.EntityCreated);

        userAudit.Metadata.AddNameid(userActor.Nameid);

        if (command.Create.FirstName is { } firstName)
        {
            user.FirstName = firstName;

            userAudit.Metadata.AddFirstName(firstName);
        }

        if (command.Create.LastName is { } lastName)
        {
            user.LastName = lastName;

            userAudit.Metadata.AddLastName(lastName);
        }

        if (command.Create.Description is { } description)
        {
            userActor.Description = description;

            userAudit.Metadata.AddDescription(userActor.Description);
        }

        if (command.Create.IsActive is true)
        {
            userActor.Activate();
        }
        else if (command.Create.IsActive is false)
        {
            userActor.Deactivate();
        }

        userAudit.Metadata.AddIsActive(userActor.IsActive);

        user.Authentication.DefaultPasswordExpirationPeriod = command.Create.Authentication?.DefaultPasswordExpirationPeriod ?? null;

        if (command.Create.Authentication?.Password is { } password)
        {
            var passwordHash = passwordHasher.Hash(new(command.Create.Authentication.Password));

            user.Authentication.SetPasswordHash(passwordHash);
        }

        user.Authentication.MarkPasswordChangeAsRequired();

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Distinct();

            foreach (var action in requestedActions)
            {
                actor.ThrowIfPermissionDenied(action);

                userActor.AddPermission(action);
            }
        }

        userAudit.Metadata.AddPermissions([.. userActor.Permissions]);

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userActor.AddPartition(partition);
            }
        }

        userAudit.Metadata.AddPartitions(userActor.PartitionGuids);

        if (command.Create.UserGroupsToAdd is { Count: > 0 } userGroups)
        {
            foreach (var userGroupGuid in userGroups.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid, EntityType.UserGroup);

                actor.ThrowIfAccessDenied(userGroup);

                user.AddUserGroup(userGroup);
            }

            userAudit.Metadata.AddUserGroupsAdded(
                [.. user.UserGroupMemberships.Select(m => m.UserGroupGuid)]);
        }

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userActor.AddPartitionAccess(partition);
            }

            userAudit.Metadata.AddPartitionAccessesAdded(
                [.. userActor.PartitionAccesses.Select(p => p.PartitionGuid)]);
        }

        userRepository.Add(user);

        auditLogRepository.Add(userAudit);

        return user.ActorGuid;
    }
}
