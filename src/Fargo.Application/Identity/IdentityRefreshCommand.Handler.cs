using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Identity;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the token refresh command by validating the existing refresh token and issuing new access and refresh tokens.
/// </summary>
/// <param name="actorService">Resolves actor information for user validation</param>
/// <param name="userRepository">Provides access to user data for validation</param>
/// <param name="tokenGenerator">Generates new access tokens</param>
/// <param name="refreshTokenGenerator">Generates new refresh tokens</param>
/// <param name="tokenHasher">Hashes refresh tokens for secure storage</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence</param>
/// <param name="unitOfWork">Provides transactional consistency for data operations</param>
/// <param name="logger">Logs the execution of the refresh process</param>
public sealed class IdentityRefreshCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<IdentityRefreshCommandHandler> logger
) : ICommandHandler<IdentityRefreshCommand, IdentityAuthResultDto>
{
    /// <summary>
    /// Processes the token refresh command by validating the old refresh token, checking user status,
    /// generating new tokens, and revoking the old refresh token. Returns a new authentication result
    /// with updated access and refresh tokens.
    /// </summary>
    /// <param name="command">The refresh command containing the refresh token to validate</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A new authentication result containing updated access and refresh tokens</returns>
    public async Task<IdentityAuthResultDto> HandleAsync(IdentityRefreshCommand command, CancellationToken cancellationToken = default)
    {
        logger.IdentityRefreshStarted();

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken);

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsable)
        {
            logger.IdentityRefreshRejectedMissionToken();

            throw new InvalidCredentialException();
        }

        var user = await userRepository.GetByGuidAsync(storedOldRefreshToken.UserGuid, cancellationToken);

        if (user is null)
        {
            logger.IdentityRefreshRejectedUserNotFound(storedOldRefreshToken.UserGuid);

            throw new InvalidCredentialException();
        }

        if (!user.IsActive)
        {
            storedOldRefreshToken.Revoke();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.IdentityRefreshRejectedUserNotActive(user.Guid);

            throw new InvalidCredentialException();
        }

        if (user.Authentication.IsPasswordChangeRequired)
        {
            storedOldRefreshToken.Revoke();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.IdentityRefreshPasswordChangeRequired(user.Guid);

            throw new UserPasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = RefreshToken.Create(user.Guid, newRefreshTokenHash);

        refreshTokenRepository.Add(storedNewRefreshToken);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var actorUser = await actorService.GetActorByGuidAndTypeAsync(user.Guid, ActorType.User, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actorUser, user.Guid, ActorType.User);

        logger.IdentityRefreshCompleted(user.Guid);

        return new IdentityAuthResultDto(
            newAccessTokenResult.AccessToken.Value,
            rawNewRefreshToken.Value,
            newAccessTokenResult.ExpiresAt
        );
    }
}
