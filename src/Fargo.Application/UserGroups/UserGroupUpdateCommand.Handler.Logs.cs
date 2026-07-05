using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

internal static partial class UserGroupUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group update flow started for user group {userGroupGuid} by actor {actorId}.")]
    public static partial void UpdateStarted(
        this ILogger logger,
        Guid userGroupGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group update mutation completed for user group {UserGroupGuid} by actor {actorId}.")]
    public static partial void UpdateCompleted(
        this ILogger logger,
        Guid userGroupGuid,
        ActorId actorId);
}
