namespace Fargo.Mcp.Configuration;

/// <summary>
/// Configuration for the Fargo MCP server, sourced from environment variables.
///
/// The server is launched on demand by an MCP host (e.g. an AI agent), so all
/// configuration is supplied through the process environment rather than files:
///
/// <list type="bullet">
///   <item><c>FARGO_SERVER</c> - base URL of the Fargo API (required).</item>
///   <item><c>FARGO_USERNAME</c> / <c>FARGO_PASSWORD</c> - credentials used to
///     log in and obtain a bearer token.</item>
///   <item><c>FARGO_ACCESS_TOKEN</c> - a pre-issued bearer token. When set it is
///     used directly and username/password are ignored.</item>
/// </list>
/// </summary>
public sealed class FargoMcpOptions
{
    public const string ServerVariable = "FARGO_SERVER";
    public const string UsernameVariable = "FARGO_USERNAME";
    public const string PasswordVariable = "FARGO_PASSWORD";
    public const string AccessTokenVariable = "FARGO_ACCESS_TOKEN";

    /// <summary>The base address of the Fargo API.</summary>
    public required Uri Server { get; init; }

    /// <summary>The user identifier (nameid) used for password authentication.</summary>
    public string? Username { get; init; }

    /// <summary>The password used for password authentication.</summary>
    public string? Password { get; init; }

    /// <summary>A pre-issued access token that bypasses username/password login.</summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// True when a static access token was supplied and should be used directly.
    /// </summary>
    public bool HasStaticToken => !string.IsNullOrWhiteSpace(AccessToken);

    /// <summary>
    /// True when username and password are both present for interactive login.
    /// </summary>
    public bool HasPasswordCredentials =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    /// <summary>
    /// Builds the options from environment variables, validating the required
    /// server URL. Throws <see cref="InvalidOperationException"/> when the server
    /// is missing or malformed so the host surfaces a clear startup error.
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
