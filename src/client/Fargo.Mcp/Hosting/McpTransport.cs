namespace Fargo.Mcp.Hosting;

/// <summary>
/// Decides which MCP transport the process should host.
/// </summary>
internal static class McpTransport
{
    public const string EnvironmentVariable = "FARGO_MCP_TRANSPORT";

    /// <summary>
    /// True when <c>--transport http</c> (or <c>--transport=http</c>) was passed,
    /// or <c>FARGO_MCP_TRANSPORT=http</c> is set. The command-line argument wins.
    /// </summary>
    public static bool IsHttp(string[] args, string? environmentValue = null)
    {
        environmentValue ??= Environment.GetEnvironmentVariable(EnvironmentVariable);

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--transport", StringComparison.OrdinalIgnoreCase)
                && i + 1 < args.Length)
            {
                return IsHttpName(args[i + 1]);
            }

            const string prefix = "--transport=";

            if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return IsHttpName(args[i][prefix.Length..]);
            }
        }

        return IsHttpName(environmentValue);
    }

    private static bool IsHttpName(string? value) =>
        string.Equals(value?.Trim(), "http", StringComparison.OrdinalIgnoreCase);
}
