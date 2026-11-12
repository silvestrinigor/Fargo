using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Tokens;


namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Command used to refresh authentication tokens using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record IdentityRefreshCommand(Token RefreshToken) : ICommand<IdentityAuthResultDto>;

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
    /// Processes the token refresh command by validating the old refresh token, checking user status,
    /// generating new tokens, and revoking the old refresh token. Returns a new authentication result
    /// with updated access and refresh tokens.
    /// </summary>
    /// <param name="command">The refresh command containing the refresh token to validate</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A new authentication result containing updated access and refresh tokens</returns>
    public async Task<IdentityAuthResultDto> HandleAsync(IdentityRefreshCommand command, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken);

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsableAt(now))
        {
            throw new InvalidCredentialsFargoApplicationException("The refresh token was missing or not usable.");
        }

        var user = await userRepository.GetByGuidAsync(storedOldRefreshToken.UserGuid, cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsFargoApplicationException($"User '{storedOldRefreshToken.UserGuid}' from the refresh token was not found.");
        }

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
