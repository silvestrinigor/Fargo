namespace Fargo.Sdk.Authentication;

public sealed class RefreshedEventArgs
{
    internal RefreshedEventArgs(string server, string nameid)
    {
        Server = server;
        Nameid = nameid;
    }

    public string Server { get; }

    public string Nameid { get; }
}
