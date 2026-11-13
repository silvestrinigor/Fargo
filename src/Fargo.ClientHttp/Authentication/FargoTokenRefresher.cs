using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.ClientAuthentication;

/// <summary>
/// Exchanges the stored refresh token for new tokens through the Fargo API
/// (<c>POST v1/identity/refresh</c>) and saves them in the <see cref="ITokenStore"/>.
/// </summary>
/// <remarks>
/// <para>
/// Nothing refreshes automatically: the application decides when to call
/// <see cref="EnsureValidAsync"/> or <see cref="RefreshAsync"/>, or opts in to
/// <see cref="RefreshingAccessTokenSource"/>.
/// </para>
/// <para>
/// The API revokes a refresh token when it is used. Two refreshes sent with the
/// same token would therefore log the session out, so refreshes through one
/// instance run one at a time, and a caller that waited re-reads the store and
/// skips the refresh when another caller already rotated the tokens. Share one
/// instance per <see cref="ITokenStore"/> (dependency injection does this).
/// </para>
/// <para>
/// A refresh the API rejects (for example an expired or revoked refresh token)
/// leaves the store unchanged and returns <see langword="false"/>, so the next
/// request fails with the API's own authentication error. Network failures are
/// not caught.
/// </para>
/// <para>
/// The <see cref="HttpClient.BaseAddress"/> of the client must be the API base address.
/// </para>
/// </remarks>
public sealed class FargoTokenRefresher
{
    /// <summary>
    /// How close to expiry <see cref="EnsureValidAsync"/> starts refreshing, so a token does not
    /// expire in flight.
    /// </summary>
    public static readonly TimeSpan DefaultExpiryBuffer = TimeSpan.FromSeconds(30);

    public const string RefreshPath = "v1/identity/refresh";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Func<HttpClient> httpClientFactory;
    private readonly ITokenStore tokenStore;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan expiryBuffer;
    private readonly SemaphoreSlim gate = new(1, 1);

    public FargoTokenRefresher(
        HttpClient httpClient,
        ITokenStore tokenStore,
        TimeProvider? timeProvider = null,
        TimeSpan? expiryBuffer = null)
        : this(ReturnClient(httpClient), tokenStore, timeProvider, expiryBuffer)
    {
    }

    public FargoTokenRefresher(
        Func<HttpClient> httpClientFactory,
        ITokenStore tokenStore,
        TimeProvider? timeProvider = null,
        TimeSpan? expiryBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(tokenStore);

        this.httpClientFactory = httpClientFactory;
        this.tokenStore = tokenStore;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.expiryBuffer = expiryBuffer ?? DefaultExpiryBuffer;
    }

    /// <summary>
    /// Refreshes the tokens when the access token has expired or expires within
    /// the expiry buffer; otherwise does nothing.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when this call refreshed the tokens; <see langword="false"/> when no
    /// refresh was needed, there are no stored tokens, or the API rejected the refresh.
    /// </returns>
    public async Task<bool> EnsureValidAsync(CancellationToken cancellationToken = default)
    {
        if (await tokenStore.GetAsync(cancellationToken) is not { } tokens || !NeedsRefresh(tokens))
        {
            return false;
        }

        await gate.WaitAsync(cancellationToken);

        try
        {
            // Another caller may have refreshed while this one waited.
            if (await tokenStore.GetAsync(cancellationToken) is not { } current || !NeedsRefresh(current))
            {
                return false;
            }

            return await RefreshCoreAsync(current, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Refreshes the tokens now, whatever their expiry.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the store holds newly refreshed tokens (from this call or a
    /// concurrent one); <see langword="false"/> when there are no stored tokens or the API rejected
    /// the refresh.
    /// </returns>
    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (await tokenStore.GetAsync(cancellationToken) is not { } before)
        {
            return false;
        }

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (await tokenStore.GetAsync(cancellationToken) is not { } current)
            {
                return false;
            }

            // Already rotated by a concurrent refresh: sending the old refresh
            // token again would be rejected.
            if (!string.Equals(current.RefreshToken, before.RefreshToken, StringComparison.Ordinal))
            {
                return true;
            }

            return await RefreshCoreAsync(current, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private bool NeedsRefresh(AuthTokens tokens)
        => tokens.ExpiresAt - expiryBuffer <= timeProvider.GetUtcNow();

    private async Task<bool> RefreshCoreAsync(AuthTokens tokens, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory();

        using var response = await httpClient.PostAsJsonAsync(
            GetRefreshUri(httpClient),
            new RefreshRequest(tokens.RefreshToken),
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        RefreshResponse? result;

        try
        {
            result = await response.Content.ReadFromJsonAsync<RefreshResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return false;
        }

        if (result is not { AccessToken: { Length: > 0 } accessToken, RefreshToken: { Length: > 0 } refreshToken, ExpiresAt: { } expiresAt })
        {
            return false;
        }

        await tokenStore.SetAsync(new AuthTokens(accessToken, refreshToken, expiresAt), cancellationToken);

        return true;
    }

    /// <summary>
    /// Keeps any path the base address has (for example a reverse-proxy prefix).
    /// </summary>
    internal static Uri GetRefreshUri(HttpClient httpClient)
    {
        var baseAddress = httpClient.BaseAddress
            ?? throw new InvalidOperationException("The HttpClient used to refresh tokens has no BaseAddress.");

        var withSlash = baseAddress.AbsoluteUri.EndsWith('/')
            ? baseAddress
            : new Uri(baseAddress.AbsoluteUri + "/");

        return new Uri(withSlash, RefreshPath);
    }

    private static Func<HttpClient> ReturnClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        return () => httpClient;
    }

    private sealed record RefreshRequest(string RefreshToken);

    private sealed record RefreshResponse(string? AccessToken, string? RefreshToken, DateTimeOffset? ExpiresAt);
}
