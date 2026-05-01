namespace Fargo.Api;

/// <summary>
/// Thrown when a network-level failure prevents the SDK from reaching the server.
/// </summary>
public sealed class FargoSdkConnectionException : FargoSdkException
{
    internal FargoSdkConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
