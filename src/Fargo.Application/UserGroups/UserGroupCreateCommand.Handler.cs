using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
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

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, parentUserGroupGuid, EntityType.UserGroup);

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

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartition(partition);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                userGroup.AddPartitionAccess(partition);
            }
        }

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(userGroup.Guid, currentActor.Guid);

        return userGroup.Guid;
    }
}
