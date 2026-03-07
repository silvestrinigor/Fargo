using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
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
    public sealed class RefreshCommandHandler(
            IUserRepository userRepository,
            ITokenGenerator tokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenHasher tokenHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<RefreshCommand, AuthResult>
    {
        /// <summary>
        /// Validates the provided refresh token, rotates it, and generates a new access token.
        /// </summary>
        /// <param name="command">The command containing the refresh token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> containing the new access token,
        /// new refresh token, and access token expiration time.
        /// </returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the refresh token is invalid, expired, or the user cannot be resolved.
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
                    newAccessTokenResult.ExpiresAt
                    );
        }
    }
}