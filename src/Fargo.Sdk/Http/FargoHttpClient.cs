using Fargo.Sdk.Configurations;
using Fargo.Sdk.Security;
using System.Net.Http.Headers;

namespace Fargo.Sdk.Http;

public sealed class FargoHttpClient
{
    private readonly HttpClient httpClient;

    private readonly FargoAuthSession session;

    public FargoHttpClient(HttpClient httpClient, FargoOptions options, FargoAuthSession session)
    {
        this.httpClient = httpClient;

        httpClient.BaseAddress = new Uri(options.BaseUrl);

        this.session = session;
    }

    private void ApplyAuthHeader()
    {
        if (session.IsAuthenticated)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        ApplyAuthHeader();
        return await httpClient.PostAsync(url, content);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        ApplyAuthHeader();
        return await httpClient.GetAsync(url);
    }
}
