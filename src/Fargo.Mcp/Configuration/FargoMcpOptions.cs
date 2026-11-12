namespace Fargo.Mcp.Configuration;

/// <summary>
/// Configuration of the Fargo MCP server, read from environment variables because an MCP host launches it on
/// demand. When <c>FARGO_ACCESS_TOKEN</c> is set it is used directly and username and password are ignored.
/// </summary>
public sealed class FargoMcpOptions
{
    public const string ServerVariable = "FARGO_SERVER";
    public const string UsernameVariable = "FARGO_USERNAME";
    public const string PasswordVariable = "FARGO_PASSWORD";
    public const string AccessTokenVariable = "FARGO_ACCESS_TOKEN";

    public required Uri Server { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? AccessToken { get; init; }

    public bool HasStaticToken => !string.IsNullOrWhiteSpace(AccessToken);

    public bool HasPasswordCredentials =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> when the server URL is missing or malformed, so the host
    /// surfaces a clear startup error.
    /// </summary>
    public static FargoMcpOptions FromEnvironment()
    {
        var server = Environment.GetEnvironmentVariable(ServerVariable);

        if (string.IsNullOrWhiteSpace(server))
        {
            throw new InvalidOperationException(
                $"The '{ServerVariable}' environment variable is required and must " +
                "contain the base URL of the Fargo API (e.g. https://localhost:7001).");
        }

        if (!Uri.TryCreate(server, UriKind.Absolute, out var serverUri))
        {
            throw new InvalidOperationException(
                $"The '{ServerVariable}' environment variable ('{server}') is not a " +
                "valid absolute URL.");
        }

        return new FargoMcpOptions
        {
            Server = serverUri,
            Username = Environment.GetEnvironmentVariable(UsernameVariable),
            Password = Environment.GetEnvironmentVariable(PasswordVariable),
            AccessToken = Environment.GetEnvironmentVariable(AccessTokenVariable)
        };
    }
}
