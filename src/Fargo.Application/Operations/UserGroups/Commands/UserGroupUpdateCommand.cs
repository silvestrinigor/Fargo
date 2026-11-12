using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.UserGroups.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Operations.UserGroups;

/// <summary>
/// Command used to update an existing user group from an API update payload.
/// </summary>
/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: on the re-parent path the
/// handler acquires a hierarchy advisory lock that must be held until commit, so
/// it runs inside a single explicit transaction owned by the pipeline.
/// </remarks>
public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid, UserGroupUpdateDto Update
) : ITransactionalCommand;

public sealed class UserGroupUpdateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupUpdateCommand>
{
    // This handler's command implements ITransactionalCommand, so the registration
    // layer wraps it in the transactional unit-of-work decorator: its whole body runs
    // inside one explicit transaction that the decorator commits once on success. It
    // must therefore NOT open its own transaction; it simply acquires the hierarchy
    // advisory lock on the re-parent path, and the ambient transaction keeps that lock
    // held until the decorator commits.
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

            // Acquire the global user-group-hierarchy lock before the cycle check so
            // the read-check-write (validate no cycle -> set parent) is atomic: the
            // lock is held until the decorator's transaction commits, so concurrent
            // re-parents cannot each pass the check and together form a cycle.
            await userGroupRepository.AcquireHierarchyLockAsync(cancellationToken);

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
