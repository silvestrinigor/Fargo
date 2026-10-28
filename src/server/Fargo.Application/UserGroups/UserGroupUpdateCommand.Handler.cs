using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupUpdateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
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

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid, EntityType.UserGroup);

        actor.ThrowIfAccessDenied(userGroup);

        var userGroupAudit = AuditLog.CreateAuditLog(actor, userGroup, AuditLogType.EntityUpdated);

        if (command.Update.Nameid is not null && userGroup.Nameid != command.Update.Nameid)
        {
            await userGroupService.ValidateUserGroupNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            userGroup.Nameid = command.Update.Nameid.Value;

            userGroupAudit.Metadata.AddNameid(userGroup.Nameid);
        }

        if (command.Update.Description is { } description)
        {
            userGroup.Description = description;

            userGroupAudit.Metadata.AddDescription(userGroup.Description);
        }

        if (command.Update.IsActive is true)
        {
            userGroup.Activate();

            userGroupAudit.Metadata.AddIsActive(userGroup.IsActive);
        }
        else if (command.Update.IsActive is false)
        {
            userGroup.Deactivate();

            userGroupAudit.Metadata.AddIsActive(userGroup.IsActive);
        }

        if (command.Update.ParentUserGroup is { } parentUserGroupGuid)
        {
            var parentUserGroup = await userGroupRepository.GetByGuidAsync(parentUserGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, parentUserGroupGuid, EntityType.UserGroup);

            actor.ThrowIfAccessDenied(parentUserGroup);

            await userGroupService.ValidateParentUserGroupAssignmentHierarchyAsync(parentUserGroup, userGroup, cancellationToken);

            userGroup.SetParentUserGroup(parentUserGroup);

            userGroupAudit.Metadata.AddParentUserGroup(userGroup.ParentUserGroupGuid);
        }
        else if (command.Update.RemoveParentUserGroup is true)
        {
            userGroup.RemoveFromParentUserGroup();

            userGroupAudit.Metadata.AddParentUserGroup(userGroup.ParentUserGroupGuid);
        }

        if (command.Update.PermissionsToAdd is { Count: > 0 } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                userGroup.AddPermission(permission);
            }

            userGroupAudit.Metadata.AddPermissionsAdded([.. permissionsToAdd.Distinct()]);
        }

        if (command.Update.PermissionsToRemove is { Count: > 0 } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove.Distinct())
            {
                actor.ThrowIfPermissionDenied(permission);

                userGroup.RemovePermission(permission);
            }

            userGroupAudit.Metadata.AddPermissionsRemoved([.. permissionsToRemove.Distinct()]);
        }

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartition(partition);
            }

            userGroupAudit.Metadata.AddPartitionsAdded([.. partitionsToAdd.Distinct()]);
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.RemovePartition(partition.Guid);
            }

            userGroupAudit.Metadata.AddPartitionsRemoved([.. partitionsToRemove.Distinct()]);
        }

        if (command.Update.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartitionAccess(partition);
            }

            userGroupAudit.Metadata.AddPartitionAccessesAdded([.. partitionAccessesToAdd]);
        }

        if (command.Update.PartitionAccessesToRemove is { Count: > 0 } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.RemovePartitionAccess(partition.Guid);
            }

            userGroupAudit.Metadata.AddPartitionAccessesRemoved([.. partitionAccessesToRemove]);
        }

        auditLogRepository.Add(userGroupAudit);
    }
}
