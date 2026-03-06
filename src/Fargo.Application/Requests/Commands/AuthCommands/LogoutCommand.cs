using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record LogoutCommand(
            Token RefreshToken
            ) : ICommand;

    public sealed class LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ITokenHasher tokenHasher,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<LogoutCommand>
    {
        public async Task Handle(LogoutCommand command, CancellationToken cancellationToken = default)
        {
            var tokenHash = tokenHasher.Hash(command.RefreshToken);

            var refreshTokenStored = await refreshTokenRepository.GetByTokenHash(tokenHash, cancellationToken);

            if (refreshTokenStored == null) return;

            refreshTokenRepository.Remove(refreshTokenStored);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}