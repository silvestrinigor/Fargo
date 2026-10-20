using Fargo.ClientHttp.Authentication;

namespace Fargo.Cli.Authentication;

/// <summary>
/// Wraps the persistent token store with an optional in-memory override used
/// for a single CLI invocation. When the user supplies <c>--username</c> and
/// <c>--password</c> on any command, the resulting tokens are kept only for the
/// duration of the process and never written to the persistent store, so an
/// existing logged-in session is left untouched.
/// </summary>
public sealed class SessionTokenStore : ITokenStore
{
    private readonly ITokenStore _persistentStore;

    private AuthTokens? _sessionOverride;

    public SessionTokenStore(FargoCliTokenStore persistentStore)
    {
        _persistentStore = persistentStore;
    }

    /// <summary>
    /// Sets tokens that live only for the current invocation. These take
    /// precedence over the persisted tokens and are never saved.
    /// </summary>
    public void SetSessionOverride(AuthTokens tokens)
    {
        _sessionOverride = tokens;
    }

    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        return _sessionOverride is not null
            ? Task.FromResult<AuthTokens?>(_sessionOverride)
            : _persistentStore.GetAsync(cancellationToken);
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        // When an invocation is running with an ad-hoc session, keep writes in
        // memory so the persisted session is not clobbered. Otherwise persist
        // normally (e.g. the `identity login` command).
        if (_sessionOverride is not null)
        {
            _sessionOverride = tokens;

            return Task.CompletedTask;
        }

        return _persistentStore.SetAsync(tokens, cancellationToken);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionOverride is not null)
        {
            _sessionOverride = null;

            return Task.CompletedTask;
        }

        return _persistentStore.ClearAsync(cancellationToken);
    }
}
