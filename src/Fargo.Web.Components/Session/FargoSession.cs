using Fargo.Api.Authentication;

namespace Fargo.Web.Components.Session;

/// <summary>
/// Blazor-scoped reactive wrapper around <see cref="IAuthenticationService"/> state.
/// Exposes <see cref="IsReady"/> and <see cref="Changed"/> so components can react to
/// login, logout, and token refresh without polling.
/// </summary>
public sealed class FargoSession : IDisposable
{
    public FargoSession(IAuthenticationService auth)
    {
        Authentication = auth;
        auth.LoggedIn += (_, _) => OnAuthChanged();
        auth.LoggedOut += (_, _) => OnAuthChanged();
        auth.Refreshed += (_, _) => OnAuthChanged();
    }

    public bool IsReady { get; private set; }

    public bool IsAuthenticated => Authentication.IsAuthenticated;

    public IAuthSession Session => Authentication.Session;

    public IAuthenticationService Authentication { get; }

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

    public void Dispose() { }

    private void OnAuthChanged() => Changed?.Invoke();
}
