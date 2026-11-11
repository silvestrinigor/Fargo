using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;

namespace Fargo.McpTester.Services;

/// <summary>
/// The logged-in Fargo user of one Blazor circuit. Holds that user's tokens and
/// refreshes them when they are about to expire. Registered as scoped, so two
/// browser tabs never share a token.
/// </summary>
public sealed class UserSession
{
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    private readonly FargoApiClient _client;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private AuthTokens? _tokens;

    public UserSession(FargoApiClient client)
    {
        _client = client;
    }

    public event Action? Changed;

    public bool IsAuthenticated => _tokens is not null;

    public string? UserName { get; private set; }

    /// <summary>Logs in with Fargo credentials. Returns an error message, or null on success.</summary>
    public async Task<string?> LoginAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.V1.Identity.Login.PostAsync(
                new IdentityLoginDto { Nameid = nameid, Password = password },
                cancellationToken: cancellationToken);

            if (!TryStore(result))
            {
                return "The Fargo API did not return a valid access token.";
            }

            UserName = nameid;
            Changed?.Invoke();

            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return $"Login failed: {ex.Message}";
        }
    }

    public void Logout()
    {
        _tokens = null;
        UserName = null;
        Changed?.Invoke();
    }

    /// <summary>Returns a valid access token, refreshing it first when needed.</summary>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var tokens = _tokens ?? throw new InvalidOperationException("Not logged in.");

            if (tokens.ExpiresAt - ExpiryBuffer > DateTimeOffset.UtcNow)
            {
                return tokens.AccessToken;
            }

            IdentityAuthResultDto? refreshed = null;

            try
            {
                refreshed = await _client.V1.Identity.Refresh.PostAsync(
                    new IdentityRefreshDto { RefreshToken = tokens.RefreshToken },
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Falls through to the "session expired" handling below.
            }

            if (!TryStore(refreshed))
            {
                Logout();
                throw new InvalidOperationException("Your Fargo session expired. Please log in again.");
            }

            return _tokens!.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool TryStore(IdentityAuthResultDto? result)
    {
        if (result?.AccessToken is null || result.RefreshToken is null || result.ExpiresAt is null)
        {
            return false;
        }

        _tokens = new AuthTokens(result.AccessToken, result.RefreshToken, result.ExpiresAt.Value);

        return true;
    }
}
