namespace Fargo.Api.Authentication;

/// <summary>
/// Thrown by <see cref="IAuthenticationManager"/> when the server requires the user
/// to change their password before the session can be established.
/// </summary>
public sealed class PasswordChangeRequiredFargoSdkException : FargoSdkApiException
{
    internal PasswordChangeRequiredFargoSdkException(string detail)
        : base(detail)
    {
    }
}
