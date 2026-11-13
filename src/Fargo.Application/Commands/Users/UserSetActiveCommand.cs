using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Commands.Users;

public sealed record UserSetActiveCommand(Guid UserGuid, bool IsActive) : ICommand;

public sealed class UserSetActiveCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserSetActiveCommand>
{
    public async Task HandleAsync(
        UserSetActiveCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ActivateUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        if (command.IsActive)
        {
            user.Actor.Activate();
        }
        else
        {
            user.Actor.Deactivate();
        }
    }
}
