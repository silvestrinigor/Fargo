using Fargo.Application.Shared.Identity;

namespace Fargo.WebIdentity;

public sealed class IdentitySession
{
    public AuthResult? CurrentAuth { get; private set; }

    public bool IsAuthenticated
        => CurrentAuth is not null &&
           CurrentAuth.ExpiresAt > DateTimeOffset.UtcNow;

    public string? AccessToken => CurrentAuth?.AccessToken;

    public void Set(AuthResult auth) => CurrentAuth = auth;

    public void Clear() => CurrentAuth = null;
}
