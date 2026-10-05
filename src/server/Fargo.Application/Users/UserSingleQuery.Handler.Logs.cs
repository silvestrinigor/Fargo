using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User single query started for user {userGuid} by actor {actorGuid}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger, Guid userGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User single query completed for user {userGuid} by actor {actorGuid}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger, Guid userGuid, Guid actorGuid, bool found);
}
