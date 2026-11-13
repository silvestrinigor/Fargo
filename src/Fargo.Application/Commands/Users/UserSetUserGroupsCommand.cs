using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.Users;

public sealed record UserSetUserGroupsCommand(Guid UserGuid, UserGroupsChangeDto Change) : ICommand;

/// <remarks>
/// Adding a user to a group hands them the permissions of that group and of its ancestors, so the
/// actor must hold all of them. Removals only need access to the group.
/// </remarks>
public sealed class UserSetUserGroupsCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserSetUserGroupsCommand>
{
    public async Task HandleAsync(
        UserSetUserGroupsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserGroupMembers);

        var (toAdd, toRemove) = ActorScopeChangeExtensions.ValidateUserGroupsChange(command.Change);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        foreach (var userGroupGuid in toAdd)
        {
            var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid);

            actor.ThrowIfAccessDenied(userGroup);

            await actor.ThrowIfMissingInheritedPermissionsAsync(userGroup, userGroupRepository, cancellationToken);

            user.AddUserGroup(userGroup);
        }

        foreach (var userGroupGuid in toRemove)
        {
            var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid);

            actor.ThrowIfAccessDenied(userGroup);

            user.RemoveUserGroup(userGroup.Guid);
        }
    }
}
