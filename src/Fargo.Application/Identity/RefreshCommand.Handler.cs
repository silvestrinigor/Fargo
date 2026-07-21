using Fargo.Application.Shared.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Identity;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class RefreshCommandHandler(
    ActorService actorService,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<RefreshCommandHandler> logger
) : ICommandHandler<RefreshCommand, AuthResult>
{
    public async Task<AuthResult> HandleAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.RefreshStarted();

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHash(oldRefreshTokenHash, cancellationToken);

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsable)
        {
            logger.RefreshRejectedMissionToken();

            throw new UnauthorizedAccessException();
        }

        var user = await userRepository.GetByGuidAsync(storedOldRefreshToken.UserGuid, cancellationToken);

        if (user is null)
        {
            logger.RefreshRejectedUserNotFound(storedOldRefreshToken.UserGuid);

            throw new UnauthorizedAccessException();
        }

        if (!user.IsActive)
        {
            storedOldRefreshToken.Revoke();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.RefreshRejectedUserNotActive(user.Guid);

            throw new UnauthorizedAccessException();
        }

        if (user.IsPasswordChangeRequired)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.RefreshPasswordChangeRequired(user.Guid);

            throw new PasswordChangeRequiredException(user.Guid);
        }

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

        var actorUserId = new ActorId(storedOldRefreshToken.UserGuid, ActorType.User);

        var actorUser = await actorService.GetActorByActorIdAsync(actorUserId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actorUser, actorUserId);

        logger.RefreshCompleted(user.Guid);

        return new AuthResult(
            newAccessTokenResult.AccessToken,
            rawNewRefreshToken,
            newAccessTokenResult.ExpiresAt);
    }
}

