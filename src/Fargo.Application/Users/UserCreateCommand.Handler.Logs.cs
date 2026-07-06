using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User create flow started by actor {actorId}.")]
    public static partial void UserCreateStarted(
        this ILogger logger, ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User create mutation completed for user {userGuid} by actor {actorId}.")]
    public static partial void UserCreateCompleted(
        this ILogger logger,
        Guid userGuid, ActorId actorId);
}
