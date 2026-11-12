using Fargo.Application.Abstractions;
using Fargo.Core.Identity.Tokens;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Operations.Identity;

public sealed record IdentityLogoutCommand(Token RefreshToken) : ICommand;

public sealed class IdentityLogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenHasher tokenHasher,
    TimeProvider timeProvider,
    ILogger<IdentityLogoutCommandHandler> logger
) : ICommandHandler<IdentityLogoutCommand>
{
    /// <summary>
    /// Completes silently when no stored token matches.
    /// </summary>
    public async Task HandleAsync(IdentityLogoutCommand command, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

        if (storedRefreshToken == null)
        {
            logger.IdentityLogoutCompletedRefreshTokenNotFound();

            return;
        }

        storedRefreshToken.Revoke(timeProvider.GetUtcNow());
    }
}

internal static partial class IdentityLogoutCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Logout flow completed without revoking a refresh token because it was not found.")]
    public static partial void IdentityLogoutCompletedRefreshTokenNotFound(this ILogger logger);
}
