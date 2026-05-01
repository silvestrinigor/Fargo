namespace Fargo.Api;

/// <summary>
/// Configuration options for the Fargo SDK. Set <see cref="Server"/> before making any requests.
/// </summary>
public sealed class FargoSdkOptions
{
    /// <summary>The base URL of the Fargo API server (e.g. <c>http://localhost:5000</c>).</summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>Optional API key sent as <c>X-Api-Key</c> on every request to identify this application.</summary>
    public string? ApiKey { get; set; }
}
