using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Commands.Users;

public sealed record UserSetPermissionsCommand(Guid UserGuid, PermissionsChangeDto Change) : ICommand;

public sealed class UserSetPermissionsCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserSetPermissionsCommand>
{
    public async Task HandleAsync(
        UserSetPermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserPermissions);

        var (toAdd, toRemove) = ActorPermissionChangeExtensions.ValidatePermissionsChange(command.Change);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        actor.ThrowIfChangingOwnPermissions(user.ActorGuid, ActorType.User);

        actor.ApplyPermissionsChange(toAdd, toRemove, user.Actor.AddPermission, user.Actor.RemovePermission);
    }
}
