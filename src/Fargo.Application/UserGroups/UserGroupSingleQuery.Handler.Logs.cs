using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

internal static partial class UserGroupSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User group single query started for user group {userGroupGuid} by actor {actorGuid}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger,
        Guid userGroupGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User group single query completed for user group {userGroupGuid} by actor {actorGuid}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger, Guid userGroupGuid,
        Guid actorGuid, bool found);
}
