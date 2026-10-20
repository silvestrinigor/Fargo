using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.ErrorHandling;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Configuration;

namespace Fargo.Mcp.Authentication;

/// <summary>
/// Owns authentication for the MCP server's Fargo API client.
///
/// Depending on configuration it either:
/// <list type="bullet">
///   <item>uses a static <c>FARGO_ACCESS_TOKEN</c> as the bearer token, or</item>
///   <item>logs in with <c>FARGO_USERNAME</c>/<c>FARGO_PASSWORD</c> and keeps the
///     resulting access token refreshed as it nears expiry.</item>
/// </list>
///
/// Tools call <see cref="EnsureAuthenticatedAsync"/> before each request so a
/// long-lived server session keeps a valid token without re-reading
/// configuration.
/// </summary>
public sealed class FargoAuthenticator
{
    // Refresh slightly before the real expiry to avoid sending a token that
    // expires in-flight.
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    // A static token has no known expiry; treat it as valid for a long time so
    // we never attempt to refresh it.
    private static readonly DateTimeOffset StaticTokenExpiry = DateTimeOffset.MaxValue;

    private readonly FargoApiClient _client;
    private readonly ITokenStore _tokenStore;
    private readonly FargoMcpOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FargoAuthenticator(
        FargoApiClient client,
        ITokenStore tokenStore,
        FargoMcpOptions options)
    {
        _client = client;
        _tokenStore = tokenStore;
        _options = options;
    }

    /// <summary>
    /// Ensures the token store holds a valid access token for the configured
    /// Fargo server, performing an initial login or a refresh when needed.
    /// </summary>
    public async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        // Anonymous mode: no credentials supplied. Leave the store empty and let
        // the API reject the request with a clear 401 if authentication is
        // required.
        if (!_options.HasStaticToken && !_options.HasPasswordCredentials)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_options.HasStaticToken)
            {
                await EnsureStaticTokenAsync(cancellationToken);
                return;
            }

            await EnsurePasswordTokenAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureStaticTokenAsync(CancellationToken cancellationToken)
    {
        var tokens = await _tokenStore.GetAsync(cancellationToken);

        if (tokens is not null && tokens.AccessToken == _options.AccessToken)
        {
            return;
        }

        await _tokenStore.SetAsync(
            new AuthTokens(_options.AccessToken!, string.Empty, StaticTokenExpiry),
            cancellationToken);
    }

    private async Task EnsurePasswordTokenAsync(CancellationToken cancellationToken)
    {
        var tokens = await _tokenStore.GetAsync(cancellationToken);

        // Still valid: nothing to do.
        if (tokens is not null && tokens.ExpiresAt - ExpiryBuffer > DateTimeOffset.UtcNow)
        {
            return;
        }

        // Try to refresh an existing (expired) token first; fall back to a full
        // login if that fails or there is nothing to refresh.
        if (tokens is not null && !string.IsNullOrEmpty(tokens.RefreshToken))
        {
            var refreshed = await TryRefreshAsync(tokens.RefreshToken, cancellationToken);

            if (refreshed)
            {
                return;
            }
        }

        await LoginAsync(cancellationToken);
    }

    private async Task<bool> TryRefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        IdentityAuthResultDto? result;

        try
        {
            result = await _client.Identity.Refresh.PostAsync(
                new IdentityRefreshDto { RefreshToken = refreshToken },
                cancellationToken: cancellationToken);
        }
        catch (FargoApiException)
        {
            return false;
        }

        return await StoreResultAsync(result, cancellationToken);
    }

    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        var result = await _client.Identity.Login.PostAsync(
            new IdentityLoginDto
            {
                Nameid = _options.Username,
                Password = _options.Password
            },
            cancellationToken: cancellationToken);

        if (!await StoreResultAsync(result, cancellationToken))
        {
            throw new InvalidOperationException(
                "Authentication with the configured FARGO_USERNAME/FARGO_PASSWORD failed: " +
                "the Fargo API did not return a valid access token.");
        }
    }

    private async Task<bool> StoreResultAsync(
        IdentityAuthResultDto? result,
        CancellationToken cancellationToken)
    {
        if (result?.AccessToken is null
            || result.RefreshToken is null
            || result.ExpiresAt is null)
        {
            return false;
        }

        await _tokenStore.SetAsync(
            new AuthTokens(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt.Value),
            cancellationToken);

        return true;
    }
}
