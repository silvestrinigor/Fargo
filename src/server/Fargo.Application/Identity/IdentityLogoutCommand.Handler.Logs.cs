using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class IdentityLogoutCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logout flow started.")]
    public static partial void IdentityLogoutStarted(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Logout flow completed without revoking a refresh token because it was not found.")]
    public static partial void IdentityLogoutCompletedRefreshTokenNotFound(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logout flow completed for user '{UserGuid}'.")]
    public static partial void IdentityLogoutCompleted(this ILogger logger, Guid userGuid);
}
