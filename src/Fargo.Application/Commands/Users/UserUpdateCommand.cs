using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Users;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Commands.Users;

public sealed record UserUpdateCommand(
    Guid UserGuid, UserUpdateDto Update
) : ICommand;

public sealed class UserUpdateCommandHandler(
    UserService userService,
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserUpdateCommand>
{
    public async Task HandleAsync(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        var userActor = user.Actor;

        if (command.Update.Nameid is not null)
        {
            await userService.ValidateUserNameidIsAvailableAsync(command.Update.Nameid.Value, cancellationToken);

            userActor.Nameid = command.Update.Nameid.Value;
        }

        if (command.Update.FirstName is { } firstName)
        {
            user.FirstName = firstName;
        }

        if (command.Update.LastName is { } lastName)
        {
            user.LastName = lastName;
        }

        if (command.Update.Description is { } description)
        {
            userActor.Description = description;
        }
    }
}
