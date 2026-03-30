namespace Fargo.Sdk;

public sealed class LoggedOutEventArgs
{
    internal LoggedOutEventArgs(string server, string nameid)
    {
        Server = server;
        Nameid = nameid;
    }

    public string Server { get; }

    public string Nameid { get; }
}
