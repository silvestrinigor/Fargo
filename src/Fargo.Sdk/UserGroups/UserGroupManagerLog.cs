using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.UserGroups;

internal static class UserGroupManagerLog
{
    private static readonly Action<ILogger, Guid, string, Exception?> _userGroupUpdateFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Error, default, "Failed to update user group {UserGroupGuid}: {Detail}");

    internal static void LogUserGroupUpdateFailed(this ILogger logger, Guid userGroupGuid, string detail)
        => _userGroupUpdateFailed(logger, userGroupGuid, detail, null);
}
