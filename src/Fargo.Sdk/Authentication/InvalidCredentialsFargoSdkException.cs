namespace Fargo.Sdk.Authentication;

/// <summary>
/// Thrown by <see cref="IAuthenticationManager"/> when the server rejects the
/// provided credentials during login.
/// </summary>
public sealed class InvalidCredentialsFargoSdkException : FargoSdkApiException
{
    internal InvalidCredentialsFargoSdkException(string detail)
        : base(detail)
    {
    }
}
