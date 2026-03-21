using System.Net.Http.Headers;
using Fargo.Web.Security;

namespace Fargo.Web.Api;

public sealed class FargoApiClientFactory(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ClientSessionAccessor sessionAccessor = sessionAccessor;

    public HttpClient Create(bool includeAccessToken = true)
    {
        var client = httpClientFactory.CreateClient("FargoApi");

        client.DefaultRequestHeaders.Authorization = null;

        if (includeAccessToken)
        {
            var accessToken = sessionAccessor.Session?.AccessToken;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return client;
    }
}