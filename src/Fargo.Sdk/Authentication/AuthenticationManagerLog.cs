using Microsoft.Extensions.Logging;

namespace Fargo.Api.Authentication;

internal static class AuthenticationManagerLog
{
    private static readonly Action<ILogger, double, Exception?> _refreshScheduled =
        LoggerMessage.Define<double>(LogLevel.Debug, default, "Next token refresh in {Minutes:F1} min");

    private static readonly Action<ILogger, Exception?> _refreshCancelled =
        LoggerMessage.Define(LogLevel.Debug, default, "Token refresh cancelled");

    private static readonly Action<ILogger, string?, Exception?> _refreshFailed =
        LoggerMessage.Define<string?>(LogLevel.Error, default, "Background token refresh failed for {Nameid}");

    internal static void LogRefreshScheduled(this ILogger logger, double minutes) => _refreshScheduled(logger, minutes, null);
    internal static void LogRefreshCancelled(this ILogger logger) => _refreshCancelled(logger, null);
    internal static void LogRefreshFailed(this ILogger logger, string? nameid, Exception ex) => _refreshFailed(logger, nameid, ex);
}
