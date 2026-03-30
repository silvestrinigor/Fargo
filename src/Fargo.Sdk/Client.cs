namespace Fargo.Sdk;

public sealed class Client : IClient, IDisposable
{
    public Client()
    {
        HttpClient = new HttpClient();
    }

    private HttpClient HttpClient { get; }

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public async Task LogInAsync(string server, string nameid, string password)
    {

    }

    public event EventHandler<LoggedOutEventArgs>? LoggedOut;

    public async Task LogOutAsync()
    {

    }

    public bool IsConnected => throw new NotImplementedException();

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}
