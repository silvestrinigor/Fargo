using Fargo.Sdk.Authentication;
using Fargo.Sdk.Http;

namespace Fargo.Sdk;

public sealed class Engine : IEngine, IDisposable
{
    public Engine()
    {
        httpClient = new HttpClient();

        var session = new AuthSession();

        fargoHttpClient = new FargoHttpClient(httpClient, session);

        var authClient = new AuthenticationClient(fargoHttpClient);

        AuthenticationManager = new AuthenticationManager(authClient, session);
    }

    private readonly HttpClient httpClient;

    private readonly FargoHttpClient fargoHttpClient;

    public IAuthenticationManager AuthenticationManager { get; }

    public async Task LogInAsync(string server, string nameid, string password, CancellationToken ct = default)
    {
        fargoHttpClient.SetBaseUrl(server);

        await AuthenticationManager.LogInAsync(nameid, password, ct);
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
