using Fargo.Application.Common;
using Fargo.Core.Identity;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class IdentityLogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenHasher tokenHasher,
    IUnitOfWork unitOfWork,
    ILogger<IdentityLogoutCommandHandler> logger
) : ICommandHandler<IdentityLogoutCommand>
{
    public async Task HandleAsync(
        IdentityLogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogoutStarted();

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken =
            await refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

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
