using Fargo.Sdk;
using Fargo.Sdk.Authentication;

namespace Fargo.Web.Components.Session;

/// <summary>
/// Blazor-scoped reactive wrapper around <see cref="IEngine"/> authentication state.
/// Exposes <see cref="IsReady"/> and <see cref="Changed"/> so components can react to
/// login, logout, and token refresh without polling.
/// </summary>
public sealed class FargoSession : IDisposable
{
    private readonly IEngine engine;

    public FargoSession(IEngine engine)
    {
        this.engine = engine;
        engine.Authentication.LoggedIn += (_, _) => OnAuthChanged();
        engine.Authentication.LoggedOut += (_, _) => OnAuthChanged();
        engine.Authentication.Refreshed += (_, _) => OnAuthChanged();
        engine.Authentication.RefreshFailed += (_, e) =>
        {
            if (e.Exception is not FargoSdkConnectionException)
            {
                _ = engine.Authentication.LogOutAsync();
                OnAuthChanged();
            }
        };
    }

    public bool IsReady { get; private set; }

    public bool IsAuthenticated => engine.Authentication.IsAuthenticated;

    public IAuthSession Session => engine.Authentication.Session;

    public IAuthenticationManager Authentication => engine.Authentication;

    /// <summary>Fires whenever authentication state changes or <see cref="MarkReady"/> is called.</summary>
    public event Action? Changed;

    /// <summary>
    /// Called by <c>SessionBootstrapper</c> once the initial restore attempt completes
    /// (whether it succeeded or failed). Triggers a UI update.
    /// </summary>
    public void MarkReady()
    {
        IsReady = true;
        Changed?.Invoke();
    }

    public void Dispose()
    {
        // Lambda subscriptions above are intentionally not unsubscribed individually;
        // the engine is scoped and disposed alongside this instance.
    }

    private void OnAuthChanged() => Changed?.Invoke();
}
