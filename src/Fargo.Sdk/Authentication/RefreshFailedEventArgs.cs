namespace Fargo.Sdk.Authentication;

public sealed class RefreshFailedEventArgs
{
    internal RefreshFailedEventArgs(string? nameid, Exception exception)
    {
        Nameid = nameid;
        Exception = exception;
    }

    public string? Nameid { get; }

    public Exception Exception { get; }
}
