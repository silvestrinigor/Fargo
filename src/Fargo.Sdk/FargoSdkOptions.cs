namespace Fargo.Sdk;

/// <summary>
/// Configuration options for the Fargo SDK. Set <see cref="Server"/> before making any requests.
/// </summary>
public sealed class FargoSdkOptions
{
    /// <summary>The base URL of the Fargo API server (e.g. <c>http://localhost:5000</c>).</summary>
    public string Server { get; set; } = string.Empty;
}
