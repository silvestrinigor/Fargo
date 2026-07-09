using Fargo.Core.Identity;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenHasher tokenHasher,
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger
) : ICommandHandler<LogoutCommand>
{
    public async Task HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogoutStarted();

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken =
            await refreshTokenRepository.GetByTokenHash(refreshTokenHash, cancellationToken);

        if (storedRefreshToken == null)
        {
            logger.LogoutCompletedRefreshTokenNotFound();

            return;
        }

        storedRefreshToken.Revoke();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogoutCompleted(storedRefreshToken.UserGuid);
    }
}
