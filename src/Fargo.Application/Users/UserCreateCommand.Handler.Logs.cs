using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UserCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User create flow started by actor {actorGuid}.")]
    public static partial void UserCreateStarted(
        this ILogger logger, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User create mutation completed for user {userGuid} by actor {actorGuid}.")]
    public static partial void UserCreateCompleted(
        this ILogger logger, Guid userGuid, Guid actorGuid);
}
