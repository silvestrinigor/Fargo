using Fargo.Core.Identity;
using Fargo.Core.Shared;
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
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow started for user {UserGuid}.", currentActor.ActorId);
        }

        var userGuid = currentActor.ActorId.Guid;

        var user = await userRepository.GetByGuidAsync(
            userGuid,
            cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Password change flow rejected because user {UserGuid} was not found.", userGuid);

            throw new UnauthorizedAccessException();
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password change flow rejected for inactive user {UserGuid}.", user.Guid);

            throw new UnauthorizedAccessException();
        }

        if (command.Passwords.CurrentPassword is null)
        {
            logger.LogWarning(
                "Password change flow rejected because the current password was missing for user {UserGuid}.",
                user.Guid);

            throw new UnauthorizedAccessException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = passwordHasher.Verify(
            user.PasswordHash,
            currentPassword
        );

        if (!isValid)
        {
            logger.LogWarning("Password change flow rejected because the current password was invalid for user {UserGuid}.", user.Guid);
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

        // TODO: ChangePasswordHash should validate if the actor is the user or the actor has access to change another user password.
        user.PasswordHash = passwordHasher.Hash(command.Passwords.NewPassword);

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

