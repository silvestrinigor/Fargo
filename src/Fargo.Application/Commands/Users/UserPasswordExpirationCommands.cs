using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Commands.Users;

public sealed record UserSetPasswordExpirationCommand(Guid UserGuid, PasswordExpirationDto Expiration) : ICommand;

/// <remarks>
/// Sets how long the user's password stays valid by default. It applies the next time the password
/// is changed; a zero period means the password must be changed immediately.
/// </remarks>
public sealed class UserSetPasswordExpirationCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserSetPasswordExpirationCommand>
{
    public async Task HandleAsync(
        UserSetPasswordExpirationCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeAnotherUserPassword);

        if (command.Expiration.ExpirationPeriod < TimeSpan.Zero)
        {
            throw new FargoApplicationException(
                "The password expiration period cannot be negative.",
                FargoErrorType.InvalidOperation);
        }

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        user.Authentication.DefaultPasswordExpirationPeriod = command.Expiration.ExpirationPeriod;
    }
}

public sealed record UserRemovePasswordExpirationCommand(Guid UserGuid) : ICommand;

/// <remarks>
/// Passwords of the user no longer expire by default. Leaves the user as it is when no period is set.
/// </remarks>
public sealed class UserRemovePasswordExpirationCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserRemovePasswordExpirationCommand>
{
    public async Task HandleAsync(
        UserRemovePasswordExpirationCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeAnotherUserPassword);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        user.Authentication.DefaultPasswordExpirationPeriod = null;
    }
}
