using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.UserGroups;

public sealed record UserGroupSetActiveCommand(Guid UserGroupGuid, bool IsActive) : ICommand;

public sealed class UserGroupSetActiveCommandHandler(
    ActorResolver actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupSetActiveCommand>
{
    public async Task HandleAsync(
        UserGroupSetActiveCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ActivateUserGroup);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid);

        actor.ThrowIfAccessDenied(userGroup);

        if (command.IsActive)
        {
            userGroup.Activate();
        }
        else
        {
            userGroup.Deactivate();
        }
    }
}
