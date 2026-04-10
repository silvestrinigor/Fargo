namespace Fargo.Sdk.Authentication;

public sealed class InvalidCredentialsFargoSdkException : FargoSdkApiException
{
    internal InvalidCredentialsFargoSdkException(string detail)
        : base(detail)
    {
    }
}
