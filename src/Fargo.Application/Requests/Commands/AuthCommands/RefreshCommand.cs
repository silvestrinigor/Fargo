using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record RefreshCommand(
            Token RefreshToken
            ) : ICommand<AuthResult>;

    public sealed class RefreshCommandHandler(
            IUserRepository userRepository,
            ITokenGenerator tokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenHasher tokenHasher,
            IRefreshTokenRepository refreshTokenRepository
            ) : ICommandHandler<RefreshCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(RefreshCommand command, CancellationToken cancellationToken = default)
        {
            var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

            var refreshTokenStored = await refreshTokenRepository.GetByTokenHash(oldRefreshTokenHash, cancellationToken);

            if (refreshTokenStored == null || refreshTokenStored.IsExpired)
                throw new UnauthorizedAccessFargoApplicationException();

            var newRefreshToken = refreshTokenGenerator.Generate();

            var newTokenHash = tokenHasher.Hash(newRefreshToken);

            refreshTokenRepository.Remove(refreshTokenStored);

            var user = await userRepository.GetByGuid(refreshTokenStored.UserGuid, cancellationToken)
                ?? throw new UnauthorizedAccessFargoApplicationException();

            var newRefreshTokenStore = new RefreshToken
            {
                UserGuid = user.Guid,
                ReplacedByTokenHash = oldRefreshTokenHash,
                TokenHash = newTokenHash
            };

            var newToken = tokenGenerator.Generate(user);

            return new AuthResult(newToken.AccessToken, newRefreshToken, newToken.ExpiresAt);
        }
    }
}