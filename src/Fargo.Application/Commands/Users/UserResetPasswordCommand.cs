using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Commands.Users;

public sealed record UserResetPasswordCommand(Guid UserGuid, PasswordResetDto Reset) : ICommand;

/// <remarks>
/// An administrator reset: the user must choose a new password at the next sign-in. An actor cannot
/// reset its own password this way, because that would skip the current-password check of the
/// identity password change.
/// </remarks>
public sealed class UserResetPasswordCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor,
    TimeProvider timeProvider
) : ICommandHandler<UserResetPasswordCommand>
{
    public async Task HandleAsync(
        UserResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeAnotherUserPassword);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        if (actor.Guid == user.ActorGuid && actor.ActorType == ActorType.User)
        {
            throw new FargoApplicationException(
                "An actor cannot reset its own password; use the identity password change instead.",
                FargoErrorType.InvalidOperation);
        }

        user.Authentication.SetPasswordHash(passwordHasher.Hash(command.Reset.Password));

        user.Authentication.MarkPasswordChangeAsRequired(timeProvider.GetUtcNow());
    }
}
