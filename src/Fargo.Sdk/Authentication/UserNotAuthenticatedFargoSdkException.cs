namespace Fargo.Sdk;

public class UserNotAuthenticatedFargoSdkException : FargoSdkException
{
    internal UserNotAuthenticatedFargoSdkException()
        : base("User is not authenticated.")
    {
    }
}
