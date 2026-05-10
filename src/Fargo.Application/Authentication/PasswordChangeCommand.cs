using Fargo.Application.Users;
using Fargo.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Authentication;

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="Passwords">
/// The current password and the new password.
/// </param>
public sealed record PasswordChangeCommand(
        UserPasswordUpdateDto Passwords
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="PasswordChangeCommand"/>.
/// </summary>
/// <remarks>
/// This handler validates the current authenticated user, verifies the
/// provided current password, updates the password hash, and resets the
/// password expiration date.
/// </remarks>
public sealed class PasswordChangeCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
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
    public async Task Handle(
            PasswordChangeCommand command,
            CancellationToken cancellationToken = default
    )
    {
        var userGuid = currentUser.UserGuid;

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

        ValidatePasswordPolicy(command.Passwords.NewPassword);

        user.PasswordHash = passwordHasher.Hash(command.Passwords.NewPassword);
        user.ResetPasswordExpiration();

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow completed for user {UserGuid}.", user.Guid);
        }
    }

    private static void ValidatePasswordPolicy(string password)
    {
        try
        {
            _ = new Password(password);
        }
        catch (ArgumentException ex)
        {
            // TODO: A password exception does not necessarily mean the password is weak.
            // TODO: If this changes, the SDK and the web should also be validated.
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}
