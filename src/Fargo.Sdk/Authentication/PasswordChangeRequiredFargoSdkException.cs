namespace Fargo.Sdk.Authentication;

public sealed class PasswordChangeRequiredFargoSdkException : FargoSdkApiException
{
    internal PasswordChangeRequiredFargoSdkException(string detail)
        : base(detail)
    {
    }
}
