using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User single query started for user {userGuid} by actor {actorId}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger,
        Guid userGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "User single query completed for user {userGuid} by actor {actorId}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger,
        Guid userGuid,
        ActorId actorId,
        bool found);
}
