using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="Passwords">
/// The current password and the new password.
/// </param>
public sealed record IdentityPasswordChangeCommand(IdentityPasswordUpdateDto Passwords) : ICommand;

/// <summary>
/// Handles the password change command by validating current credentials and updating user authentication information.
/// </summary>
/// <param name="userRepository">Provides access to user data.</param>
/// <param name="passwordHasher">Hashes and verifies password values.</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence for token invalidation.</param>
public sealed class IdentityPasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository
) : ICommandHandler<IdentityPasswordChangeCommand>
{
    /// <summary>
    /// Processes the password change command by validating current credentials, updating the password hash,
    /// resetting password expiration, and rotating authentication version. All existing refresh tokens are revoked.
    /// </summary>
    /// <param name="command">The password change command containing user credentials and new password</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
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

        user.Authentication.SetPasswordHash(passwordHasher.Hash(command.Passwords.NewPassword));

        user.Authentication.ResetPasswordExpiration();

        user.Authentication.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuidAsync(user.ActorGuid, cancellationToken);

        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }
    }
}
