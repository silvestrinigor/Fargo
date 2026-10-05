using Fargo.Core.Informations;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class IdentityPasswordChangeCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow started for user '{userNameid}'.")]
    public static partial void IdentityPasswordChangeStarted(this ILogger logger, string userNameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because user '{nameid}' was not found.")]
    public static partial void IdentityPasswordChangeUserNotFound(this ILogger logger, Nameid nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Passoword change flow rejected because the provided nameid '{nameid}' format is invalid.")]
    public static partial void IdentityPasswordChangeRejectedInvalidNameId(
        this ILogger logger, string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected for inactive user '{userGuid}'.")]
    public static partial void IdentityPasswordChangeUserInactive(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because the current password was invalid for user '{userGuid}'.")]
    public static partial void IdentityPasswordChangeInvalidPassword(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow completed for user '{userGuid}'.")]
    public static partial void IdentityPasswordChangeCompleted(this ILogger logger, Guid userGuid);
}
