using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.UserGroups;

/// <remarks>
/// Unlike users and service accounts, a group has no identity of its own to compare with the actor,
/// so the self-change rule does not apply: an actor that belongs to the group is held back only by
/// having to hold each permission it changes.
/// </remarks>
public sealed record UserGroupSetPermissionsCommand(Guid UserGroupGuid, PermissionsChangeDto Change) : ICommand;

public sealed class UserGroupSetPermissionsCommandHandler(
    ActorResolver actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupSetPermissionsCommand>
{
    public async Task HandleAsync(
        UserGroupSetPermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserGroupPermissions);

        var (toAdd, toRemove) = ActorPermissionChangeExtensions.ValidatePermissionsChange(command.Change);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        actor.ApplyPermissionsChange(toAdd, toRemove, userGroup.AddPermission, userGroup.RemovePermission);
    }
}
