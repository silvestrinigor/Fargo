namespace Fargo.Sdk.Authentication;

public sealed class LoggedOutEventArgs
{
    internal LoggedOutEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    public string Nameid { get; }
}
