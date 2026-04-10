using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.AuthCommands;

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="Passwords">
/// The current password and the new password.
/// </param>
public sealed record PasswordChangeCommand(
        UserPasswordUpdateModel Passwords
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
        ICurrentUser currentUser
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
        var user = await userRepository.GetByGuid(
                currentUser.UserGuid,
                cancellationToken
                ) ?? throw new UnauthorizedAccessFargoApplicationException();

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var currentPassword = command.Passwords.CurrentPassword
            ?? throw new InvalidPasswordFargoApplicationException();

        var isValid = passwordHasher.Verify(
                user.PasswordHash,
                currentPassword
                );

        if (!isValid)
        {
            throw new InvalidPasswordFargoApplicationException();
        }

        ValidatePasswordPolicy(command.Passwords.NewPassword);

        user.PasswordHash = passwordHasher.Hash(command.Passwords.NewPassword);
        user.ResetPasswordExpiration();

        await unitOfWork.SaveChanges(cancellationToken);
    }

    private static void ValidatePasswordPolicy(string password)
    {
        try
        {
            _ = new Password(password);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}
