using Fargo.Application.Common;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the password change command by validating current credentials and updating user authentication information.
/// </summary>
/// <param name="userRepository">Provides access to user data.</param>
/// <param name="passwordHasher">Hashes and verifies password values.</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence for token invalidation.</param>
/// <param name="unitOfWork">Provides transactional consistency for data operations.</param>
/// <param name="logger">Logs the execution of the password change process.</param>
public sealed class IdentityPasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
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
        logger.IdentityPasswordChangeStarted(command.Passwords.Nameid);

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

        if (!user.IsActive)
        {
            logger.IdentityPasswordChangeUserInactive(user.Guid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = user.Authentication.PasswordHash is not null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, new(currentPassword));

        if (!isValid)
        {
            logger.IdentityPasswordChangeInvalidPassword(user.Guid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        user.Authentication.SetPasswordHash(passwordHasher.Hash(command.Passwords.NewPassword));

        user.Authentication.ResetPasswordExpiration();

        user.Authentication.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuidAsync(user.Guid, cancellationToken);

        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.IdentityPasswordChangeCompleted(user.Guid);
    }
}
