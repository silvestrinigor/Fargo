using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

internal static partial class IdentityLoginCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Login flow started for user '{nameId}'.")]
    public static partial void IdentityLoginStarted(
        this ILogger logger, string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the provided nameid '{nameid}' format is invalid.")]
    public static partial void IdentityLoginRejectedInvalidNameId(
        this ILogger logger, string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the user '{nameId}' was not found.")]
    public static partial void IdentityLoginRejectedUserNotFound(
        this ILogger logger, string nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the user '{nameId}' is not active.")]
    public static partial void IdentityLoginRejectedUserNotActive(
        this ILogger logger, string nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected for user '{userGuid}' because the password format is invalid.")]
    public static partial void IdentityLoginRejectedInvalidPasswordFormat(
        this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the password was invalid for user '{UserGuid}'.")]
    public static partial void IdentityLoginRejectedInvalidPassword(
        this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow requires password change for user '{UserGuid}'.")]
    public static partial void IdentityLoginRejectedPasswordChangeRequired(
        this ILogger logger, Guid userGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Login flow completed for user '{nameId}'.")]
    public static partial void IdentityLoginCompleted(
        this ILogger logger, string nameId);
}
