using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

internal static partial class UserGroupDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group delete flow started for user group {userGroupGuid} by actor {actorId}.")]
    public static partial void DeleteStarted(
        this ILogger logger,
        Guid userGroupGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group delete mutation completed for user group {userGroupGuid} by actor {actorId}.")]
    public static partial void DeleteCompleted(
        this ILogger logger,
        Guid userGroupGuid,
        ActorId actorId);
}
