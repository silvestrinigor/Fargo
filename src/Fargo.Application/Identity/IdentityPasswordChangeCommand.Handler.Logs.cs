using Fargo.Core.Informations;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class IdentityPasswordChangeCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow started for user '{userNameid}'.")]
    public static partial void PasswordChangeStarted(this ILogger logger, string userNameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because user '{nameid}' was not found.")]
    public static partial void PasswordChangeUserNotFound(this ILogger logger, Nameid nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Passoword change flow rejected because the provided nameid {nameid} format is invalid.")]
    public static partial void PasswordChangeRejectedInvalidNameId(
        this ILogger logger, string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected for inactive user '{userGuid}'.")]
    public static partial void PasswordChangeUserInactive(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password change flow rejected because the current password was invalid for user '{userGuid}'.")]
    public static partial void PasswordChangeInvalidPassword(this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password change flow completed for user '{userGuid}'.")]
    public static partial void PasswordChangeCompleted(this ILogger logger, Guid userGuid);
}
