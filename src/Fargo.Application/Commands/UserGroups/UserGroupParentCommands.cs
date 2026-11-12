using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.UserGroups;

/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: the handler takes the user-group hierarchy
/// advisory lock, which must be held until commit. Setting the parent a group already has does
/// nothing.
/// </remarks>
public sealed record UserGroupSetParentCommand(Guid UserGroupGuid, Guid ParentUserGroupGuid) : ITransactionalCommand;

/// <remarks>
/// Placing a group under a parent hands the parent's permissions, and those of its ancestors, to every
/// member of the group, so the actor must hold all of them.
/// </remarks>
public sealed class UserGroupSetParentCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupSetParentCommand>
{
    // The command is transactional, so the registration layer wraps this handler in the
    // transactional unit-of-work decorator: the whole body runs in one transaction that the
    // decorator commits once on success. The handler must therefore NOT open its own; it only takes
    // the hierarchy advisory lock, which the ambient transaction keeps until commit.
    public async Task HandleAsync(
        UserGroupSetParentCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserGroupParent);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        if (userGroup.ParentUserGroupGuid == command.ParentUserGroupGuid)
        {
            return;
        }

        var parentUserGroup = await userGroupRepository.GetByGuidAsync(command.ParentUserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentUserGroup, command.ParentUserGroupGuid);

        actor.ThrowIfAccessDenied(parentUserGroup);

        await actor.ThrowIfMissingInheritedPermissionsAsync(parentUserGroup, userGroupRepository, cancellationToken);

        // The global lock makes the read-check-write (no cycle, then set the parent) atomic: it is
        // held until the decorator's transaction commits, so concurrent re-parents cannot each pass
        // the check and together form a cycle.
        await userGroupRepository.AcquireHierarchyLockAsync(cancellationToken);

        await userGroupService.ValidateParentUserGroupAssignmentHierarchyAsync(parentUserGroup, userGroup, cancellationToken);

        userGroup.SetParentUserGroup(parentUserGroup);
    }
}

/// <remarks>
/// A group without a parent is left as it is.
/// </remarks>
public sealed record UserGroupRemoveParentCommand(Guid UserGroupGuid) : ICommand;

public sealed class UserGroupRemoveParentCommandHandler(
    ActorResolver actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupRemoveParentCommand>
{
    public async Task HandleAsync(
        UserGroupRemoveParentCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserGroupParent);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        if (userGroup.ParentUserGroupGuid is null)
        {
            return;
        }

        userGroup.RemoveFromParentUserGroup();
    }
}
