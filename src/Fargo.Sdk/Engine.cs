using Fargo.Sdk.Authentication;
using Fargo.Sdk.Http;

namespace Fargo.Sdk;

public sealed class Engine : IEngine
{
    public Engine()
    {
        httpClient = new HttpClient();

        var session = new AuthSession();

        fargoHttpClient = new FargoHttpClient(httpClient, session);

        var authClient = new AuthenticationClient(fargoHttpClient);

        Authentication = new AuthenticationManager(authClient, session);
    }

    public IAuthenticationManager Authentication { get; }

    public async Task LogInAsync(string server, string nameid, string password, CancellationToken ct = default)
    {
        if (Authentication.IsAuthenticated)
        {
            await Authentication.LogOutAsync(ct);
        }

        fargoHttpClient.SetBaseUrl(server);

        await Authentication.LogInAsync(nameid, password, ct);
    }

    public Task LogOutAsync(CancellationToken ct = default)
    {
        return Authentication.LogOutAsync(ct);
    }

    public void Dispose()
    {
        Authentication.Dispose();
        httpClient.Dispose();
    }

    private readonly HttpClient httpClient;

    private readonly FargoHttpClient fargoHttpClient;
}
