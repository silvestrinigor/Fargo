using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class LogoutCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logout flow started.")]
    public static partial void LogoutStarted(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Logout flow completed without revoking a refresh token because it was not found.")]
    public static partial void LogoutCompletedRefreshTokenNotFound(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logout flow completed for user {UserGuid}.")]
    public static partial void LogoutCompleted(this ILogger logger, Guid userGuid);
}
