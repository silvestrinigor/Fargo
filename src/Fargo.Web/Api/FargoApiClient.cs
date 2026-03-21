using System.Text.Json;

namespace Fargo.Web.Api;

public abstract class FargoApiClient
{
    private readonly HttpClient client;

    protected FargoApiClient(IHttpClientFactory httpClientFactory)
    {
        client = httpClientFactory.CreateClient("FargoApi");
    }

    protected HttpClient Client => client;

    protected static JsonSerializerOptions Json => FargoJsonSerializerOptions.Default;

    protected static async Task<T> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        var value = await response.Content.ReadFromJsonAsync<T>(
            Json,
            cancellationToken);

        return value ?? throw new HttpRequestException("The API returned an empty response.");
    }

    protected static Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
}
