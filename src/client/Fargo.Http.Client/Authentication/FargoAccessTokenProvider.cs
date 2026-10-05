using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Http.Client.Authentication;

public sealed class FargoAccessTokenProvider : IAccessTokenProvider
{
    private readonly ITokenStore _tokenStore;

    public FargoAccessTokenProvider(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _tokenStore.GetAsync(cancellationToken);

        if (tokens is null)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        return tokens.AccessToken;
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new();
}
