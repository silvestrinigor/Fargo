using Fargo.Core.Identity;
using Fargo.Core.Shared;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity.Commands.Handlers;

/// <summary>
/// Handles the execution of <see cref="PasswordChangeCommand"/>.
/// </summary>
public sealed class PasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentUser,
    ILogger<PasswordChangeCommandHandler> logger
) : ICommandHandler<PasswordChangeCommand>
{
    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="command">
    /// The command containing the current password and the new password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or the user is inactive.
    /// </exception>
    /// <exception cref="InvalidPasswordFargoApplicationException">
    /// Thrown when the current password is invalid.
    /// </exception>
    public async Task HandleAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default)
    {
        var userGuid = currentUser.ActorId;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow started for user {UserGuid}.", userGuid);
        }

        var user = await userRepository.GetByGuid(
            userGuid,
            cancellationToken
        );

        if (user is null)
        {
            logger.LogWarning("Password change flow rejected because user {UserGuid} was not found.", userGuid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password change flow rejected for inactive user {UserGuid}.", user.Guid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (command.Passwords.CurrentPassword is null)
        {
            logger.LogWarning(
                "Password change flow rejected because the current password was missing for user {UserGuid}.",
                user.Guid);
            throw new InvalidPasswordFargoApplicationException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = passwordHasher.Verify(
            user.PasswordHash,
            currentPassword
        );

        if (!isValid)
        {
            logger.LogWarning("Password change flow rejected because the current password was invalid for user {UserGuid}.", user.Guid);
            throw new InvalidPasswordFargoApplicationException();
        }

        try
        {
            _ = new Password(command.Passwords.NewPassword);
        }
        catch (ArgumentException ex)
        {
            // TODO: not aways the reason is weak password.
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }

        // TODO: ChangePasswordHash should validate if the actor is the user or the actor has access to change another user password.
        user.ChangePasswordHash(passwordHasher.Hash(command.Passwords.NewPassword));
        user.ResetPasswordExpiration();
        user.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);
        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow completed for user {UserGuid}.", user.Guid);
        }
    }
}

