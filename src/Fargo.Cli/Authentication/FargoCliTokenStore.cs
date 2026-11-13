using Fargo.Cli.Configurations;
using Fargo.Client.Authentication;
using ktsu.CredentialCache;
using System.Text.Json;

namespace Fargo.Cli.Authentication;

/// <summary>
/// Persists the session token in the OS credential cache, bound to the server it was issued for.
/// A token obtained against one server is never returned for another, so logging in with
/// <c>--server A</c> and then running against the configured server B does not reuse A's token.
/// </summary>
/// <remarks>
/// A credential store that cannot be used (for example, no <c>libsecret</c> on Linux) surfaces as a
/// <see cref="CredentialStoreUnavailableException"/> instead of the backend's native exception.
/// </remarks>
public class FargoCliTokenStore : ITokenStore
{
    private readonly PersonaGUID personaGuid;

    private readonly ActiveServerContext activeServer;

    public FargoCliTokenStore(
        IFargoCliConfigurationStore configurationStore,
        ActiveServerContext activeServer)
    {
        this.activeServer = activeServer;

        var config = configurationStore.Load();

        if (config.CredentialId == null)
        {
            config.CredentialId = CredentialCache.CreatePersonaGUID();

            configurationStore.Save(config);
        }

        personaGuid = PersonaGUID.Create(config.CredentialId);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        UseCredentialStore(() => CredentialCache.Instance.Remove(personaGuid));

        return Task.CompletedTask;
    }

    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        var credential = UseCredentialStore(() =>
            CredentialCache.Instance.TryGet(personaGuid, out Credential? found) ? found : null);

        if (credential is null)
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        if (credential is not CredentialWithToken tokenCredential)
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        var stored = TryDeserialize(tokenCredential.Token);

        if (stored is null)
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        // Only hand back the token if it belongs to the server this invocation
        // is talking to. Otherwise behave as if there is no stored session.
        if (!SameServer(stored.Server, activeServer.Server))
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        return Task.FromResult(stored.Tokens);
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        var payload = new StoredToken
        {
            Server = NormalizeServer(activeServer.Server),
            Tokens = tokens
        };

        var json = JsonSerializer.Serialize(payload);

        UseCredentialStore(() => CredentialCache.Instance.AddOrReplace(personaGuid, new CredentialWithToken
        {
            Token = CredentialToken.Create(json)
        }));

        return Task.CompletedTask;
    }

    /// <remarks>
    /// The native backends fail in many ways (a missing library throws <see cref="DllNotFoundException"/>
    /// wrapped in a <see cref="TypeInitializationException"/>, a missing D-Bus session throws other
    /// exceptions), so every failure except cancellation is treated as an unavailable store.
    /// </remarks>
    internal static T UseCredentialStore<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new CredentialStoreUnavailableException(exception);
        }
    }

    private static void UseCredentialStore(Action action)
        => UseCredentialStore(() =>
        {
            action();
            return true;
        });

    private static StoredToken? TryDeserialize(string json)
    {
        try
        {
            var stored = JsonSerializer.Deserialize<StoredToken>(json);

            // A payload from before tokens were server-scoped has no server and
            // no tokens; treat it as absent so the user simply logs in again.
            if (stored?.Tokens is null)
            {
                return null;
            }

            return stored;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool SameServer(string? storedServer, Uri? activeServer)
    {
        if (storedServer is null || activeServer is null)
        {
            return false;
        }

        return string.Equals(
            storedServer,
            NormalizeServer(activeServer),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeServer(Uri? server)
    {
        if (server is null)
        {
            return null;
        }

        // GetComponents with Scheme|Host|Port yields "https://host:port" with no
        // path or trailing slash, giving a stable key per server endpoint.
        return server.GetComponents(
            UriComponents.Scheme | UriComponents.Host | UriComponents.Port,
            UriFormat.UriEscaped);
    }

    private sealed class StoredToken
    {
        public string? Server { get; set; }

        public AuthTokens? Tokens { get; set; }
    }
}
