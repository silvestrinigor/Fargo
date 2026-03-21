using System.Net.Http.Headers;

namespace Fargo.Web.Api;

public abstract class FargoApiClientBase(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ClientSessionAccessor sessionAccessor = sessionAccessor;

    protected HttpClient CreateClient(bool requireAuthentication = true)
    {
        var client = httpClientFactory.CreateClient("FargoApi");

        if (!requireAuthentication)
        {
            return client;
        }

        var accessToken = sessionAccessor.Session?.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("The current user is not authenticated.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}
