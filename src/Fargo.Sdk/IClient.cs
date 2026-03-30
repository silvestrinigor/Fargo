namespace Fargo.Sdk;

public interface IClient
{
    event EventHandler<LoggedInEventArgs>? LoggedIn;

    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    bool IsConnected { get; }
}
