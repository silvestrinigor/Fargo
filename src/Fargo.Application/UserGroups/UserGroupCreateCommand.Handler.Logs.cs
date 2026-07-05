using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

internal static partial class UserGroupCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group create flow started by actor {actorId}.")]
    public static partial void CreateStarted(
        this ILogger logger,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User group create mutation completed for user group {userGroupGuid} by actor {actorId}.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid userGroupGuid,
        ActorId actorId);
}
