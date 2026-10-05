using Fargo.Application.Common;
using Fargo.Core.Identity;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the authentication logout command by revoking the provided refresh token.
/// </summary>
/// <param name="refreshTokenRepository">Provides access to refresh token data.</param>
/// <param name="tokenHasher">Hashes refresh tokens for secure lookup.</param>
/// <param name="unitOfWork">Provides transactional consistency for data operations.</param>
/// <param name="logger">Logs the execution of the logout process.</param>
public sealed class IdentityLogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenHasher tokenHasher,
    IUnitOfWork unitOfWork,
    ILogger<IdentityLogoutCommandHandler> logger
) : ICommandHandler<IdentityLogoutCommand>
{
    /// <summary>
    /// Processes the logout command by hashing the provided refresh token and revoking the corresponding stored token.
    /// If no matching token is found, the operation completes silently.
    /// </summary>
    /// <param name="command">The logout command containing the refresh token to revoke</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    public async Task HandleAsync(IdentityLogoutCommand command, CancellationToken cancellationToken = default)
    {
        logger.IdentityLogoutStarted();

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

        if (storedRefreshToken == null)
        {
            logger.IdentityLogoutCompletedRefreshTokenNotFound();

            return;
        }

        storedRefreshToken.Revoke();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.IdentityLogoutCompleted(storedRefreshToken.UserGuid);
    }
}
