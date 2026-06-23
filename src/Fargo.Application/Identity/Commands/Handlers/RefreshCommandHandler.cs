using Fargo.Application.Shared.Identity;
using Fargo.Core.Identity;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity.Commands.Handlers;

/// <summary>
/// Handles the execution of <see cref="RefreshCommand"/>.
/// </summary>
/// <remarks>
/// This handler validates the provided refresh token, resolves the
/// associated user, rotates the refresh token, and generates a new
/// access token.
/// </remarks>
public sealed class RefreshCommandHandler(
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthorizationContextFactory authorizationContextFactory,
    IUnitOfWork unitOfWork,
    ILogger<RefreshCommandHandler> logger
) : ICommandHandler<RefreshCommand, AuthResult>
{
    /// <summary>
    /// Validates the provided refresh token, rotates it, and generates a new access token.
    /// </summary>
    /// <param name="command">
    /// The command containing the refresh token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the new access token,
    /// new refresh token, and access token expiration time.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the refresh token is invalid, expired, the user cannot
    /// be resolved, or the user is inactive.
    /// </exception>
    public async Task<AuthResult> HandleAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Refresh flow started.");

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHash(
            oldRefreshTokenHash,
            cancellationToken
        );

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsable)
        {
            logger.LogWarning("Refresh flow rejected because the refresh token was missing or not usable.");
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var user = await userRepository.GetByGuid(
            storedOldRefreshToken.UserGuid,
            cancellationToken
        );

        if (user is null)
        {
            logger.LogWarning(
                "Refresh flow rejected because user {UserGuid} from the refresh token was not found.",
                storedOldRefreshToken.UserGuid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (!user.IsActive)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogWarning("Refresh flow rejected for inactive user {UserGuid}; old refresh token was revoked.", user.Guid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (user.IsPasswordChangeRequired)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Refresh flow requires password change for user {UserGuid}; old refresh token was revoked.", user.Guid);
            }
            throw new PasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var authorization = await authorizationContextFactory.CreateFromUser(user, cancellationToken);

        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = newRefreshTokenHash
        };

        storedOldRefreshToken.ReplaceWith(newRefreshTokenHash);

        refreshTokenRepository.Add(storedNewRefreshToken);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Refresh flow completed for user {UserGuid}. IsAdmin: {IsAdmin}. PermissionCount: {PermissionCount}. PartitionAccessCount: {PartitionAccessCount}.",
                user.Guid,
                authorization.IsAdmin,
                authorization.PermissionActions.Count,
                authorization.PartitionAccesses.Count);
        }

        return new AuthResult(
            newAccessTokenResult.AccessToken,
            rawNewRefreshToken,
            newAccessTokenResult.ExpiresAt,
            authorization.IsAdmin,
            authorization.PermissionActions,
            authorization.PartitionAccesses
        );
    }
}

