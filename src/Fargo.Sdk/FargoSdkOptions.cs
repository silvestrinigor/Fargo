namespace Fargo.Sdk;

/// <summary>
/// Configuration options for the Fargo SDK gRPC transport.
/// </summary>
public sealed class FargoSdkOptions
{
    /// <summary>The Fargo gRPC service address.</summary>
    public Uri? Address { get; set; }

    /// <summary>Optional default deadline applied to SDK gRPC calls.</summary>
    public TimeSpan? DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Optional bearer token provider used to add authorization metadata to gRPC calls.</summary>
    public Func<CancellationToken, ValueTask<string?>>? BearerTokenProvider { get; set; }
}
