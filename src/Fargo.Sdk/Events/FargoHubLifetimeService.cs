using Fargo.Sdk.Authentication;

namespace Fargo.Sdk.Events;

/// <summary>
/// Optional service that automatically connects the hub after login and disconnects on logout.
/// Register via <c>builder.WithHubLifetime()</c>; resolve it once at startup to activate the subscriptions.
/// </summary>
public sealed class FargoHubLifetimeService : IDisposable
{
    public FargoHubLifetimeService(IAuthenticationService auth, IFargoEventHub hub, FargoSdkOptions options)
    {
        this.auth = auth;
        this.hub = hub;
        this.options = options;

        auth.LoggedIn += OnLoggedIn;
        auth.LoggedOut += OnLoggedOut;
    }

    private readonly IAuthenticationService auth;
    private readonly IFargoEventHub hub;
    private readonly FargoSdkOptions options;

    public void Dispose()
    {
        auth.LoggedIn -= OnLoggedIn;
        auth.LoggedOut -= OnLoggedOut;
    }

    private void OnLoggedIn(object? sender, LoggedInEventArgs e)
        => _ = hub.ConnectAsync(options.Server, () => Task.FromResult(auth.Session.AccessToken));

    private void OnLoggedOut(object? sender, LoggedOutEventArgs e)
        => _ = hub.DisconnectAsync();
}
