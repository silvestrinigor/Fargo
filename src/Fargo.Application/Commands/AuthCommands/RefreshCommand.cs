using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;

namespace Fargo.Application.Commands.AuthCommands;

/// <summary>
/// Command used to refresh authentication tokens using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record RefreshCommand(
        Token RefreshToken
        ) : ICommand<AuthResult>;

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
        ActorService actorService,
        IUnitOfWork unitOfWork
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
    public async Task<AuthResult> Handle(
            RefreshCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHash(
                oldRefreshTokenHash,
                cancellationToken
                );

        if (storedOldRefreshToken == null || storedOldRefreshToken.IsExpired)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var user = await userRepository.GetByGuid(
                storedOldRefreshToken.UserGuid,
                cancellationToken
                ) ?? throw new UnauthorizedAccessFargoApplicationException();

        if (!user.IsActive)
        {
            refreshTokenRepository.Remove(storedOldRefreshToken);
            await unitOfWork.SaveChanges(cancellationToken);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var actor = (UserActor)(await actorService.GetActorByGuid(user.Guid, cancellationToken))!;

        // TODO: Validate if the refreshToken type should be Token or should create a new struct especifcy for the refreshToken.
        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = newRefreshTokenHash
        };

        refreshTokenRepository.Remove(storedOldRefreshToken);

        refreshTokenRepository.Add(storedNewRefreshToken);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChanges(cancellationToken);

        return new AuthResult(
                newAccessTokenResult.AccessToken,
                rawNewRefreshToken,
                newAccessTokenResult.ExpiresAt,
                actor.IsAdmin,
                actor.IsAdmin ? [] : actor.PermissionActions,
                actor.IsAdmin ? [] : actor.PartitionAccesses
                );
    }
}
