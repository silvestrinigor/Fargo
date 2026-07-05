using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User delete flow started for user {userGuid} by actor {actorId}.")]
    public static partial void UserDeleteStarted(
        this ILogger logger,
        Guid userGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User delete mutation completed for user {userGuid} by actor {actorId}.")]
    public static partial void UserDeleteCompleted(
        this ILogger logger,
        Guid userGuid,
        ActorId actorId);
}
