using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Command used to delete a user group.
/// </summary>
public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

public sealed class UserGroupDeleteCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task HandleAsync(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUserGroup);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        await userGroupService.ValidateUserGroupCanBeDeletedAsync(userGroup, cancellationToken);

        userGroupRepository.Remove(userGroup);
    }
}
