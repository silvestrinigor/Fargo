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
        var tokens = await _tokenStore.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Fargo is not authenticated.");

        return tokens.AccessToken;
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new();
}
