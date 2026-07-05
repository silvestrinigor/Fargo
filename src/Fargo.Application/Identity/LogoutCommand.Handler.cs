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
        logger.LogInformation("Logout flow started.");

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken = await refreshTokenRepository.GetByTokenHash(
            refreshTokenHash, cancellationToken);

        if (storedRefreshToken == null)
        {
            logger.LogWarning("Logout flow completed without revoking a refresh token because it was not found.");

            return;
        }

        storedRefreshToken.Revoke();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Logout flow completed for user {UserGuid}.", storedRefreshToken.UserGuid);
        }
    }
}

