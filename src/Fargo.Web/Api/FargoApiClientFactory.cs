using System.Net.Http.Headers;

namespace Fargo.Web.Api;

public sealed class FargoApiClientFactory(
    IHttpClientFactory httpClientFactory,
    FargoSession session)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly FargoSession session = session;

    public HttpClient Create(bool includeAccessToken = true)
    {
        var client = httpClientFactory.CreateClient("FargoApi");

        client.DefaultRequestHeaders.Authorization = null;

        if (includeAccessToken)
        {
            var accessToken = session.Session.AccessToken;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return client;
    }
}
