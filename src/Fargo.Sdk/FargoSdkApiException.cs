namespace Fargo.Sdk;

/// <summary>
/// Base class for exceptions thrown when the API returns a domain error.
/// </summary>
public class FargoSdkApiException : FargoSdkException
{
    internal FargoSdkApiException(string detail)
        : base(detail)
    {
    }
}
