using Microsoft.Extensions.Logging;

namespace Fargo.Api.Http;

internal static class FargoSdkHttpClientLog
{
    private static readonly Action<ILogger, string, string, Exception?> _request =
        LoggerMessage.Define<string, string>(LogLevel.Debug, default, "{Method} {Url}");

    private static readonly Action<ILogger, string, string, int, Exception?> _response =
        LoggerMessage.Define<string, string, int>(LogLevel.Debug, default, "{Method} {Url} -> {StatusCode}");

    internal static void LogRequest(this ILogger logger, string method, string url) =>
        _request(logger, method, url, null);

    internal static void LogResponse(this ILogger logger, string method, string url, int statusCode) =>
        _response(logger, method, url, statusCode, null);
}
