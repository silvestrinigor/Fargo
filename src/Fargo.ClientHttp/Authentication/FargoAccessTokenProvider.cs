using Fargo.ClientAuthentication;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.ClientHttp.Authentication;

/// <summary>
/// Supplies Kiota requests with the access token from the registered
/// <see cref="IFargoAccessTokenSource"/>.
/// </summary>
/// <remarks>
/// With the default source the stored token is sent as is; the application
/// decides when to refresh (see <see cref="FargoTokenRefresher"/>).
/// </remarks>
public sealed class FargoAccessTokenProvider : IAccessTokenProvider
{
    private readonly IFargoAccessTokenSource _tokenSource;

    public FargoAccessTokenProvider(IFargoAccessTokenSource tokenSource)
    {
        _tokenSource = tokenSource;
    }

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        // Kiota sends no Authorization header for an empty token.
        return await _tokenSource.GetAccessTokenAsync(cancellationToken) ?? string.Empty;
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new();
}
