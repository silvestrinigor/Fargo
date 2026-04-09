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

    public async Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default)
    {
        if (Authentication.IsAuthenticated)
        {
            await Authentication.LogOutAsync(cancellationToken);
        }

        fargoHttpClient.SetBaseUrl(server);

        await Authentication.LogInAsync(nameid, password, cancellationToken);
    }

    public Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        return Authentication.LogOutAsync(cancellationToken);
    }

    public void Dispose()
    {
        Authentication.Dispose();
        httpClient.Dispose();
    }

    private readonly HttpClient httpClient;

    private readonly FargoHttpClient fargoHttpClient;
}
