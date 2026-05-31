using Fargo.HttpClient;
using System.Net.Http.Headers;

namespace Fargo.WebPlayground;

public sealed class PlaygroundApiClientFactory(
    IHttpClientFactory httpClientFactory,
    PlaygroundAuthSession authSession)
{
    public const string HttpClientName = "Fargo.Web.Playground.Api";

    public async ValueTask<IFargoHttpClient?> CreateAsync()
    {
        var auth = await authSession.LoadAsync();

        if (auth is null || !authSession.IsAuthenticated)
        {
            return null;
        }

        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            auth.AccessToken);

        return new FargoHttpClient(httpClient);
    }
}
