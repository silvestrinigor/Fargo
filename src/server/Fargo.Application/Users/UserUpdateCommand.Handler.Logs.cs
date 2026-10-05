using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User update flow started for user {userGuid} by actor {actorGuid}.")]
    public static partial void UpdateStarted(
        this ILogger logger, Guid userGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User update mutation completed for user {userGuid} by actor {actorGuid}.")]
    public static partial void UpdateCompleted(
        this ILogger logger, Guid userGuid, Guid actorGuid);
}
