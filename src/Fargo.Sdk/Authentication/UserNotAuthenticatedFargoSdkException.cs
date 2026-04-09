namespace Fargo.Sdk;

public class UserNotAuthenticatedFargoSdkException : Exception
{
    internal UserNotAuthenticatedFargoSdkException()
        : base("User is not authenticated.")
    {
    }
}
