namespace Fargo.Sdk.Authentication;

public sealed class PasswordChangedEventArgs
{
    internal PasswordChangedEventArgs(string server, string nameid)
    {
        Server = server;
        Nameid = nameid;
    }

    public string Server { get; }

    public string Nameid { get; }
}
