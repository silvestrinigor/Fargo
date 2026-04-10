using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Authentication;

internal static class AuthenticationManagerLog
{
    private static readonly Action<ILogger, string, Exception?> _loggedIn =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Logged in as {Nameid}");

    private static readonly Action<ILogger, string, Exception?> _loggedOut =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Logged out ({Nameid})");

    private static readonly Action<ILogger, string, Exception?> _refreshed =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Token refreshed for {Nameid}");

    private static readonly Action<ILogger, double, Exception?> _refreshScheduled =
        LoggerMessage.Define<double>(LogLevel.Debug, default, "Next token refresh in {Minutes:F1} min");

    private static readonly Action<ILogger, Exception?> _refreshCancelled =
        LoggerMessage.Define(LogLevel.Debug, default, "Token refresh cancelled");

    private static readonly Action<ILogger, string?, Exception?> _refreshFailed =
        LoggerMessage.Define<string?>(LogLevel.Error, default, "Background token refresh failed for {Nameid}");

    private static readonly Action<ILogger, string, Exception?> _passwordChanged =
        LoggerMessage.Define<string>(LogLevel.Debug, default, "Password changed for {Nameid}");

    internal static void LogLoggedIn(this ILogger logger, string nameid) => _loggedIn(logger, nameid, null);
    internal static void LogLoggedOut(this ILogger logger, string nameid) => _loggedOut(logger, nameid, null);
    internal static void LogRefreshed(this ILogger logger, string nameid) => _refreshed(logger, nameid, null);
    internal static void LogRefreshScheduled(this ILogger logger, double minutes) => _refreshScheduled(logger, minutes, null);
    internal static void LogRefreshCancelled(this ILogger logger) => _refreshCancelled(logger, null);
    internal static void LogRefreshFailed(this ILogger logger, string? nameid, Exception ex) => _refreshFailed(logger, nameid, ex);
    internal static void LogPasswordChanged(this ILogger logger, string nameid) => _passwordChanged(logger, nameid, null);
}
