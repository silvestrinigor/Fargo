using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Commands.Handlers;

internal static partial class RefreshCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow started.")]
    public static partial void RefreshStarted(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected because the refresh token was missing or not usable.")]
    public static partial void RefreshRejectedMissionToken(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected because user {userGuid} from the refresh token was not found.")]
    public static partial void RefreshRejectedUserNotFound(
        this ILogger logger,
        Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh flow rejected for inactive user {userGuid}; old refresh token was revoked.")]
    public static partial void RefreshRejectedUserNotActive(
        this ILogger logger,
        Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow requires password change for user {userGuid}; old refresh token was revoked.")]
    public static partial void RefreshPasswordChangeRequired(
        this ILogger logger,
        Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refresh flow completed for user {userGuid}.")]
    public static partial void RefreshCompleted(
        this ILogger logger,
        Guid userGuid);
}
