using Fargo.Infrastructure.Client.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.Client.Http;

public abstract class FargoHttpClientBase(HttpClient http)
{
    protected readonly HttpClient Http = http;
    protected static readonly JsonSerializerOptions JsonOptions = FargoJsonSerializerOptions.Default;

    protected async Task<T?> GetAsync<T>(string uri, CancellationToken ct)
    {
        var response = await Http.GetAsync(uri, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        await EnsureSuccess(response, ct);

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<IReadOnlyCollection<T>> GetCollectionAsync<T>(string uri, CancellationToken ct)
    {
        var response = await Http.GetAsync(uri, ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return [];

        await EnsureSuccess(response, ct);

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<T>>(JsonOptions, ct)
            ?? [];
    }

    protected async Task<T> PostAsync<T>(string uri, object body, CancellationToken ct)
    {
        var response = await Http.PostAsJsonAsync(uri, body, JsonOptions, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct))!;
    }

    protected async Task PostAsync(string uri, object body, CancellationToken ct)
    {
        var response = await Http.PostAsJsonAsync(uri, body, JsonOptions, ct);

        await EnsureSuccess(response, ct);
    }

    protected async Task PutAsync(string uri, object body, CancellationToken ct)
    {
        var response = await Http.PutAsJsonAsync(uri, body, JsonOptions, ct);

        await EnsureSuccess(response, ct);
    }

    protected async Task PatchAsync(string uri, object body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, uri)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        var response = await Http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);
    }

    protected async Task DeleteAsync(string uri, CancellationToken ct)
    {
        var response = await Http.DeleteAsync(uri, ct);

        await EnsureSuccess(response, ct);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: ct);
        }
        catch
        {
        }

        if (problem is not null)
            throw new HttpRequestException(problem.Detail);

        response.EnsureSuccessStatusCode();
    }
}