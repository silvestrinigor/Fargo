using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Authentication;

internal static class AuthenticationServiceLog
{
    private static readonly Action<ILogger, string, Exception?> _loggedIn =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Logged in as {Nameid}");

    private static readonly Action<ILogger, string, Exception?> _loggedOut =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Logged out ({Nameid})");

    private static readonly Action<ILogger, string, Exception?> _refreshed =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Token refreshed for {Nameid}");

    private static readonly Action<ILogger, string, Exception?> _passwordChanged =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Password changed for {Nameid}");

    internal static void LogLoggedIn(this ILogger logger, string nameid) => _loggedIn(logger, nameid, null);
    internal static void LogLoggedOut(this ILogger logger, string nameid) => _loggedOut(logger, nameid, null);
    internal static void LogRefreshed(this ILogger logger, string nameid) => _refreshed(logger, nameid, null);
    internal static void LogPasswordChanged(this ILogger logger, string nameid) => _passwordChanged(logger, nameid, null);
}
