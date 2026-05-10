using Fargo.Domain.Tokens;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Authentication;

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
/// <remarks>
/// This handler invalidates the provided refresh token if it exists.
/// If the token is not found, the operation completes silently.
/// </remarks>
public sealed class LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        ILogger<LogoutCommandHandler> logger
        ) : ICommandHandler<LogoutCommand>
{
    /// <summary>
    /// Invalidates the provided refresh token.
    /// </summary>
    /// <param name="command">
    /// The command containing the refresh token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous logout operation.
    /// </returns>
    /// <remarks>
    /// If the refresh token does not exist in storage, the method returns
    /// without throwing an exception.
    /// </remarks>
    public async Task Handle(
            LogoutCommand command,
            CancellationToken cancellationToken = default
            )
    {
        logger.LogInformation("Logout flow started.");

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken = await refreshTokenRepository.GetByTokenHash(
                refreshTokenHash,
                cancellationToken
                );

        if (storedRefreshToken == null)
        {
            logger.LogWarning("Logout flow completed without removing a refresh token because it was not found.");
            return;
        }

        refreshTokenRepository.Remove(storedRefreshToken);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Logout flow completed for user {UserGuid}.", storedRefreshToken.UserGuid);
        }
    }
}
