using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Fargo.Web.Api;

public abstract class FargoApiClientBase(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor,
    IOptions<JsonOptions> httpJsonOptions)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ClientSessionAccessor sessionAccessor = sessionAccessor;
    private readonly JsonSerializerOptions jsonSerializerOptions = httpJsonOptions.Value.SerializerOptions;

    protected HttpClient CreateClient(bool requireAuthentication = true)
    {
        var client = httpClientFactory.CreateClient("FargoApi");

        client.DefaultRequestHeaders.Authorization = null;

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

    protected Task<T?> GetFromJsonAsync<T>(
        string requestUri,
        bool requireAuthentication = true,
        CancellationToken cancellationToken = default) =>
        CreateClient(requireAuthentication)
            .GetFromJsonAsync<T>(requestUri, jsonSerializerOptions, cancellationToken);

    protected Task<HttpResponseMessage> PostAsJsonAsync<T>(
        string requestUri,
        T value,
        bool requireAuthentication = true,
        CancellationToken cancellationToken = default) =>
        CreateClient(requireAuthentication)
            .PostAsJsonAsync(requestUri, value, jsonSerializerOptions, cancellationToken);

    protected Task<HttpResponseMessage> PutAsJsonAsync<T>(
        string requestUri,
        T value,
        bool requireAuthentication = true,
        CancellationToken cancellationToken = default) =>
        CreateClient(requireAuthentication)
            .PutAsJsonAsync(requestUri, value, jsonSerializerOptions, cancellationToken);

    protected Task<T?> ReadFromJsonAsync<T>(
        HttpContent content,
        CancellationToken cancellationToken = default) =>
        content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken);

    protected HttpContent CreateJsonContent<T>(T value) =>
        JsonContent.Create(value, options: jsonSerializerOptions);
}