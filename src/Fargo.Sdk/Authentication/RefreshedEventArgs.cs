namespace Fargo.Sdk.Authentication;

public sealed class RefreshedEventArgs
{
    internal RefreshedEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    public string Nameid { get; }
}
