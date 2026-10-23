using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupCreateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupCreateCommandHandler> logger
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateUserGroup);

        await userGroupService.ValidateUserGroupNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var userGroup = UserGroup.CreateUserGroup(command.Create.Nameid);

        var userGroupAudit = AuditLog.CreateAuditLog(actor, userGroup, AuditLogType.EntityCreated);

        userGroupAudit.Metadata.AddNameid(userGroup.Nameid);

        userGroup.Description = command.Create.Description ?? Description.Empty;

        userGroupAudit.Metadata.AddDescription(userGroup.Description);

        if (command.Create.IsActive is true)
        {
            userGroup.Activate();
        }
        else if (command.Create.IsActive is false)
        {
            userGroup.Deactivate();
        }

        userGroupAudit.Metadata.AddIsActive(userGroup.IsActive);

        if (command.Create.ParentUserGroup is { } parentUserGroupGuid)
        {
            var parentUserGroup = await userGroupRepository.GetByGuidAsync(parentUserGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, parentUserGroupGuid, EntityType.UserGroup);

            actor.ThrowIfAccessDenied(parentUserGroup);

            userGroup.SetParentUserGroup(parentUserGroup);
        }

        userGroupAudit.Metadata.AddParentUserGroup(userGroup.ParentUserGroupGuid);

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Distinct();

            foreach (var action in requestedActions)
            {
                actor.ThrowIfPermissionDenied(action);

                userGroup.AddPermission(action);
            }
        }

        userGroupAudit.Metadata.AddPermissions([.. userGroup.Permissions]);

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartition(partition);
            }
        }

        userGroupAudit.Metadata.AddPartitions(userGroup.PartitionGuids);

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartitionAccess(partition);
            }

            userGroupAudit.Metadata.AddPartitionAccessesAdded(
                [.. userGroup.PartitionAccesses.Select(p => p.PartitionGuid)]);
        }

        userGroupRepository.Add(userGroup);

        auditLogRepository.Add(userGroupAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(userGroup.Guid, currentActor.Guid);

        return userGroup.Guid;
    }
}
