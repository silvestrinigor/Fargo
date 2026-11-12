using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.UserGroups;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Commands.UserGroups;

public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid, UserGroupUpdateDto Update
) : ICommand;

public sealed class UserGroupUpdateCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
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
    }
}