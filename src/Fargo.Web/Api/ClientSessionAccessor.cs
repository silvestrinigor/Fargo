using Fargo.Web.Security;

namespace Fargo.Web.Api;

public sealed class ClientSessionAccessor
{
    public AuthenticatedSession? Session { get; private set; }

    public bool IsReady { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Session?.AccessToken);

    public event Action? Changed;

    public void SetSession(AuthenticatedSession? session)
    {
        Session = session;
        IsReady = true;
        Changed?.Invoke();
    }

    public void MarkReady()
    {
        IsReady = true;
        Changed?.Invoke();
    }

    public void Clear()
    {
        SetSession(null);
    }
}
