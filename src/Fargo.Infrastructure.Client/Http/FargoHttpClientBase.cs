using Fargo.Infrastructure.Client.Clients.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.Client.Http;

public abstract class FargoHttpClientBase(HttpClient http)
{
    protected readonly HttpClient Http = http;

    protected static readonly JsonSerializerOptions JsonOptions =
        FargoJsonSerializerOptions.Default;

    protected async Task<T?> GetAsync<T>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<IReadOnlyCollection<T>> GetCollectionAsync<T>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return [];
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<T>>(
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    protected async Task<TResponse> PostAsync<TResponse>(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync(
            uri,
            body,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return (await response.Content.ReadFromJsonAsync<TResponse>(
            JsonOptions,
            cancellationToken))!;
    }

    protected async Task PostAsync(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync(
            uri,
            body,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected async Task PutAsync(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.PutAsJsonAsync(
            uri,
            body,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected async Task PatchAsync(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, uri)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        using var response = await Http.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected async Task DeleteAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.DeleteAsync(uri, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
                cancellationToken: cancellationToken);
        }
        catch
        {
        }

        if (problem is not null)
        {
            throw new HttpRequestException(problem.Detail);
        }

        response.EnsureSuccessStatusCode();
    }
}
