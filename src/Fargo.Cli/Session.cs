namespace Fargo.Cli;

public sealed class Session : IAsyncDisposable
{
    //public FargoConnection Connection { get; }

    public string? UserNameid { get; private set; }

    public bool ShouldExit { get; private set; }

    //public bool IsConnected => Connection.IsConnected;

    public bool IsAuthenticated => UserNameid is not null;

    public void Exit()
    {
        ShouldExit = true;
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
