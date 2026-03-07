using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
    /// <summary>
    /// Command used to log out a user by invalidating a refresh token.
    /// </summary>
    /// <param name="RefreshToken">
    /// The refresh token provided by the client.
    /// </param>
    public sealed record LogoutCommand(
            Token RefreshToken
            ) : ICommand;

    /// <summary>
    /// Handles the execution of <see cref="LogoutCommand"/>.
    /// </summary>
    public sealed class LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ITokenHasher tokenHasher,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<LogoutCommand>
    {
        /// <summary>
        /// Invalidates the provided refresh token.
        /// </summary>
        /// <param name="command">The command containing the refresh token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public async Task Handle(
                LogoutCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

            var storedRefreshToken = await refreshTokenRepository.GetByTokenHash(
                    refreshTokenHash,
                    cancellationToken
                    );

            if (storedRefreshToken == null)
            {
                return;
            }

            refreshTokenRepository.Remove(storedRefreshToken);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}