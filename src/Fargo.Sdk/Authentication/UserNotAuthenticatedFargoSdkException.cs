namespace Fargo.Sdk.Authentication;

/// <summary>
/// Thrown when an operation that requires an active session is called
/// while the user is not authenticated.
/// </summary>
public class UserNotAuthenticatedFargoSdkException : FargoSdkException
{
    internal UserNotAuthenticatedFargoSdkException()
        : base("User is not authenticated.")
    {
    }
}
