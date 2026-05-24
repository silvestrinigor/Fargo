using Fargo.HttpContracts;

namespace Fargo.Web.Identity;

public sealed class IdentitySession
{
    public AuthDto? CurrentAuth { get; private set; }

    public bool IsAuthenticated
        => CurrentAuth is not null &&
           CurrentAuth.ExpiresAt > DateTimeOffset.UtcNow;

    public string? AccessToken => CurrentAuth?.AccessToken;

    public void Set(AuthDto auth) => CurrentAuth = auth;

    public void Clear() => CurrentAuth = null;
}
