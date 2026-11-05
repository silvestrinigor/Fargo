using Fargo.Application.Common;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Operations.Users;

/// <summary>
/// Command used to delete a user.
/// </summary>
public sealed record UserDeleteCommand(Guid UserGuid) : ICommand;

public sealed class UserDeleteCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserDeleteCommand>
{
    public async Task HandleAsync(
        UserDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        if (actor.Guid == user.ActorGuid && actor.ActorType == ActorType.User)
        {
            throw new FargoApplicationException($"The user '{user.ActorGuid}' cannot delete their own user.");
        }

        user.Actor.ValidateCanBeDeleted();

        userRepository.Remove(user);
    }
}
