using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.ErrorHandling;
using Fargo.ClientHttp.Models;

namespace Fargo.Cli.Authentication;

/// <summary>
/// Refreshes the stored access token when it has expired (or is about to),
/// before a command's request is sent. If there is no token, or the refresh
/// fails, it quietly leaves the store as-is so the command can proceed and
/// surface the real authentication error.
/// </summary>
public static class TokenRefresher
{
    // Refresh slightly before the real expiry to avoid sending a token that
    // expires in-flight.
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Ensures the stored token is valid for the active server. Returns true if
    /// a refresh was performed successfully.
    /// </summary>
    public static async Task<bool> EnsureValidAsync(
        FargoApiClient client,
        ITokenStore tokenStore,
        CancellationToken cancellationToken = default)
    {
        var tokens = await tokenStore.GetAsync(cancellationToken);

        if (tokens is null)
        {
            // Nothing stored for this server; let the command hit 401 if it
            // needs auth.
            return false;
        }

        if (tokens.ExpiresAt - ExpiryBuffer > DateTimeOffset.UtcNow)
        {
            // Still valid.
            return false;
        }

        IdentityAuthResultDto? result;

        try
        {
            result = await client.Identity.Refresh.PostAsync(
                new IdentityRefreshDto { RefreshToken = tokens.RefreshToken },
                cancellationToken: cancellationToken);
        }
        catch (FargoApiException)
        {
            // Refresh token is invalid/expired too. Leave the store untouched;
            // the command will fail with a clear 401 and the user can log in.
            return false;
        }

        if (result?.AccessToken is null
            || result.RefreshToken is null
            || result.ExpiresAt is null)
        {
            return false;
        }

        await tokenStore.SetAsync(
            new AuthTokens(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt.Value),
            cancellationToken);

        return true;
    }
}
