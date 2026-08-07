using Fargo.Core.Identity;
using Fargo.Core.Security;
using Fargo.Core.Shared.Security;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class PasswordChangeCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<PasswordChangeCommandHandler> logger
) : ICommandHandler<PasswordChangeCommand>
{
    public async Task HandleAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.PasswordChangeStarted(currentActor.Guid);

        var user = await userRepository.GetByGuidAsync(currentActor.Guid, cancellationToken);

        if (user is null)
        {
            logger.PasswordChangeUserNotFound(currentActor.Guid);

            throw new UnauthorizedAccessException();
        }

        if (!user.IsActive)
        {
            logger.PasswordChangeUserInactive(user.Guid);

            throw new UnauthorizedAccessException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = user.Authentication.PasswordHash is not null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, new(currentPassword));

        if (!isValid)
        {
            logger.PasswordChangeInvalidPassword(user.Guid);

            throw new UnauthorizedAccessException();
        }

        try
        {
            _ = new Password(command.Passwords.NewPassword);
        }
        catch (ArgumentException)
        {
            // TODO: not aways the reason is weak password.
            throw new UnauthorizedAccessException();
        }

        var password = new Password(command.Passwords.NewPassword);
        // TODO: ChangePasswordHash should validate if the actor is the user or the actor has access to change another user password.
        user.Authentication.SetPasswordHash(passwordHasher.Hash(password));

        user.Authentication.ResetPasswordExpiration();

        user.Authentication.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuidAsync(user.Guid, cancellationToken);

        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.PasswordChangeCompleted(user.Guid);
    }
}
