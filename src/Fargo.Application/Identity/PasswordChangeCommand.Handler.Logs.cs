using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class LogoutCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow started for user {UserGuid}.")]
    public static partial void PasswordChangeStarted(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because user {UserGuid} was not found.")]
    public static partial void PasswordChangeUserNotFound(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected for inactive user {UserGuid}.")]
    public static partial void PasswordChangeUserInactive(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because the current password was invalid for user {UserGuid}.")]
    public static partial void PasswordChangeInvalidPassword(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow completed for user {UserGuid}.")]
    public static partial void PasswordChangeCompleted(this ILogger logger, Guid userGuid);
}
