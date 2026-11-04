using Fargo.Application.Common;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;
using Fargo.Core.Identity.Credentials;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the password change command by validating current credentials and updating user authentication information.
/// </summary>
/// <param name="userRepository">Provides access to user data.</param>
/// <param name="passwordHasher">Hashes and verifies password values.</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence for token invalidation.</param>
/// <param name="logger">Logs the execution of the password change process.</param>
public sealed class IdentityPasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<IdentityPasswordChangeCommandHandler> logger
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
            logger.IdentityPasswordChangeRejectedInvalidNameId(command.Passwords.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            logger.IdentityPasswordChangeUserNotFound(nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!user.Actor.IsActive)
        {
            logger.IdentityPasswordChangeUserInactive(user.ActorGuid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = user.Authentication.PasswordHash is not null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, new(currentPassword));

        if (!isValid)
        {
            logger.IdentityPasswordChangeInvalidPassword(user.ActorGuid);

            throw new InvalidCredentialsFargoApplicationException();
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

internal static partial class IdentityPasswordChangeCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because user '{nameid}' was not found.")]
    public static partial void IdentityPasswordChangeUserNotFound(this ILogger logger, Nameid nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Passoword change flow rejected because the provided nameid '{nameid}' format is invalid.")]
    public static partial void IdentityPasswordChangeRejectedInvalidNameId(
        this ILogger logger, string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected for inactive user '{userGuid}'.")]
    public static partial void IdentityPasswordChangeUserInactive(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because the current password was invalid for user '{userGuid}'.")]
    public static partial void IdentityPasswordChangeInvalidPassword(this ILogger logger, Guid userGuid);
}
