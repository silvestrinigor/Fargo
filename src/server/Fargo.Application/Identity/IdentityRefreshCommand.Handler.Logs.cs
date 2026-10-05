using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class IdentityRefreshCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow started.")]
    public static partial void IdentityRefreshStarted(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected because the refresh token was missing or not usable.")]
    public static partial void IdentityRefreshRejectedMissionToken(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected because user '{userGuid}' from the refresh token was not found.")]
    public static partial void IdentityRefreshRejectedUserNotFound(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected for inactive user '{userGuid}'; old refresh token was revoked.")]
    public static partial void IdentityRefreshRejectedUserNotActive(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow requires password change for user '{userGuid}'; old refresh token was revoked.")]
    public static partial void IdentityRefreshPasswordChangeRequired(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow completed for user '{userGuid}'.")]
    public static partial void IdentityRefreshCompleted(this ILogger logger, Guid userGuid);
}
