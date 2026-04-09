namespace Fargo.Sdk.Authentication;

public class UserNotAuthenticatedFargoSdkException : FargoSdkException
{
    internal UserNotAuthenticatedFargoSdkException()
        : base("User is not authenticated.")
    {
    }
}
