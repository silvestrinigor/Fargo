using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Commands.Handlers;

internal static partial class LoginCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Login flow started for user {nameId}.")]
    public static partial void LoginStarted(
        this ILogger logger,
        string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the provided nameid {nameid} format is invalid.")]
    public static partial void LoginRejectedInvalidNameId(
        this ILogger logger,
        string nameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the user {nameId} was not found.")]
    public static partial void LoginRejectedUserNotFound(
        this ILogger logger,
        string nameid);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login flow rejected because the user {nameId} is not active.")]
    public static partial void LoginRejectedUserNotActive(
        this ILogger logger,
        string nameid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Login flow completed for user {nameId}.")]
    public static partial void LoginCompleted(
        this ILogger logger,
        string nameId);
}
