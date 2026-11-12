using Fargo.Application.Abstractions;
using Fargo.Application.Dtos.Identity;
using Fargo.Application.Exceptions.Identity;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.Identity;

public sealed record IdentityPasswordChangeCommand(IdentityPasswordUpdateDto Passwords) : ICommand;

public sealed class IdentityPasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    TimeProvider timeProvider
) : ICommandHandler<IdentityPasswordChangeCommand>
{
    /// <summary>
    /// Revokes all existing refresh tokens of the user.
    /// </summary>
    public async Task HandleAsync(IdentityPasswordChangeCommand command, CancellationToken cancellationToken = default)
    {
        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Passwords.Nameid);
        }
        catch (ArgumentException)
        {
            throw new InvalidCredentialsFargoApplicationException($"Nameid '{command.Passwords.Nameid}' has an invalid format.");
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsFargoApplicationException($"User '{nameid}' was not found.");
        }

        if (!user.Actor.IsActive)
        {
            throw new InvalidCredentialsFargoApplicationException($"User '{user.ActorGuid}' is not active.");
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = user.Authentication.PasswordHash is not null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, new(currentPassword));

        if (!isValid)
        {
            throw new InvalidCredentialsFargoApplicationException($"The current password does not match for user '{user.ActorGuid}'.");
        }

        var now = timeProvider.GetUtcNow();

        user.Authentication.SetPasswordHash(passwordHasher.Hash(command.Passwords.NewPassword));

        user.Authentication.ResetPasswordExpiration(now);

        user.Authentication.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuidAsync(user.ActorGuid, cancellationToken);

        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsableAt(now)))
        {
            refreshToken.Revoke(now);
        }
    }
}
