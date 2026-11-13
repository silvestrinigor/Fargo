using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Dtos.Identity;
using Fargo.Application.Exceptions.Identity;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Commands.Identity;

public sealed record IdentityRefreshCommand(Token RefreshToken) : ICommand<IdentityAuthResultDto>;

public sealed class IdentityRefreshCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<IdentityRefreshCommand, IdentityAuthResultDto>
{
    /// <summary>
    /// Rotates the refresh token: the old one is revoked when the new one is issued.
    /// </summary>
    public async Task<IdentityAuthResultDto> HandleAsync(IdentityRefreshCommand command, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken);

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsableAt(now))
        {
            throw new InvalidCredentialsFargoApplicationException("The refresh token was missing or not usable.");
        }

        var user = await userRepository.GetByGuidAsync(storedOldRefreshToken.UserGuid, cancellationToken) ?? throw new InvalidCredentialsFargoApplicationException($"User '{storedOldRefreshToken.UserGuid}' from the refresh token was not found.");
        if (!user.Actor.IsActive)
        {
            storedOldRefreshToken.Revoke(now);

            // Persist the token revocation before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"User '{user.ActorGuid}' is not active; the old refresh token was revoked.");
        }

        if (user.Authentication.IsPasswordChangeRequiredAt(now))
        {
            storedOldRefreshToken.Revoke(now);

            // Persist the token revocation before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UserPasswordChangeRequiredFargoApplicationException(user.ActorGuid);
        }

        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = RefreshToken.Create(user.ActorGuid, newRefreshTokenHash, now);

        refreshTokenRepository.Add(storedNewRefreshToken);

        storedOldRefreshToken.Revoke(now);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var actorUser = await actorService.GetActorByGuidAndTypeAsync(user.ActorGuid, ActorType.User, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actorUser, user.ActorGuid, ActorType.User);

        return new IdentityAuthResultDto(
            newAccessTokenResult.AccessToken.Value,
            rawNewRefreshToken.Value,
            newAccessTokenResult.ExpiresAt
        );
    }
}
