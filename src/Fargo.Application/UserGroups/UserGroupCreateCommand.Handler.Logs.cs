using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

internal static partial class UserGroupCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group create flow started by actor {actorGuid}.")]
    public static partial void CreateStarted(
        this ILogger logger, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group create mutation completed for user group {userGroupGuid} by actor {actorGuid}.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid userGroupGuid, Guid actorGuid);
}
