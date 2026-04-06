namespace Fargo.Sdk.Authentication;

public sealed class PasswordChangedEventArgs
{
    internal PasswordChangedEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    public string Nameid { get; }
}
