using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Users;

internal static class UserManagerLog
{
    private static readonly Action<ILogger, Guid, string, Exception?> _userUpdateFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Error, default, "Failed to update user {UserGuid}: {Detail}");

    internal static void LogUserUpdateFailed(this ILogger logger, Guid userGuid, string detail)
        => _userUpdateFailed(logger, userGuid, detail, null);
}
