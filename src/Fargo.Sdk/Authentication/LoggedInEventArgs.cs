namespace Fargo.Sdk.Authentication;

public sealed class LoggedInEventArgs
{
    internal LoggedInEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    public string Nameid { get; }
}
