using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Identity;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class IdentityRefreshCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<IdentityRefreshCommandHandler> logger
) : ICommandHandler<IdentityRefreshCommand, AuthResult>
{
    public async Task<AuthResult> HandleAsync(
        IdentityRefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.RefreshStarted();

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken);

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

        if (user.Authentication.IsPasswordChangeRequired)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.RefreshPasswordChangeRequired(user.Guid);

            throw new PasswordChangeRequiredException(user.Guid);
        }

        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = RefreshToken.Create(user.Guid, newRefreshTokenHash);

        refreshTokenRepository.Add(storedNewRefreshToken);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var actorUser = await actorService.GetActorByGuidAndTypeAsync(user.Guid, ActorType.User, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actorUser, user.Guid, ActorType.User);

        logger.RefreshCompleted(user.Guid);

        return new AuthResult(
            newAccessTokenResult.AccessToken.Value,
            rawNewRefreshToken.Value,
            newAccessTokenResult.ExpiresAt);
    }
}
