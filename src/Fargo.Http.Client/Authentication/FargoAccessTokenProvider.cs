using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Http.Client.Authentication;

public sealed class FargoAccessTokenProvider(
    Func<CancellationToken, Task<string>> accessTokenProvider)
    : IAccessTokenProvider
{
    public AllowedHostsValidator AllowedHostsValidator { get; } = new();

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return await accessTokenProvider(cancellationToken);
    }
}
