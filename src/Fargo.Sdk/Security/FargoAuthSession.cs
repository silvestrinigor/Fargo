namespace Fargo.Sdk.Security;

public sealed class FargoAuthSession
{
    public string AccessToken { get; private set; } = string.Empty;

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public void SetToken(string token)
    {
        AccessToken = token;
    }

    public void Clear()
    {
        AccessToken = string.Empty;
    }
}
