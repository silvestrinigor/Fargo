using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.HttpApi.Client.Common;

public sealed class FargoHttpClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
{
    public async Task<FargoHttpResponse<TResponse>> GetAsync<TResponse>(string uri, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken);

        return await CreateResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<FargoHttpResponse<IReadOnlyCollection<TResponse>>> GetCollectionAsync<TResponse>(string uri, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new FargoHttpResponse<IReadOnlyCollection<TResponse>>(
                [],
                null,
                response);
        }

        return await CreateResponseAsync<IReadOnlyCollection<TResponse>>(response, cancellationToken);
    }

    public async Task<FargoHttpResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await httpClient.PostAsJsonAsync(
                uri,
                request,
                jsonSerializerOptions,
                cancellationToken);

        return await CreateResponseAsync<TResponse>(
            response,
            cancellationToken);
    }

    public async Task<FargoHttpResponse<TResponse>> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpRequestMessage requestMessage =
            new(HttpMethod.Patch, uri)
            {
                Content = JsonContent.Create(
                    request,
                    options: jsonSerializerOptions)
            };

        HttpResponseMessage response =
            await httpClient.SendAsync(
                requestMessage,
                cancellationToken);

        requestMessage.Dispose();

        return await CreateResponseAsync<TResponse>(
            response,
            cancellationToken);
    }

    public async Task<FargoHttpResponse> PatchAsync<TRequest>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpRequestMessage requestMessage =
            new(HttpMethod.Patch, uri)
            {
                Content = JsonContent.Create(
                    request,
                    options: jsonSerializerOptions)
            };

        HttpResponseMessage response =
            await httpClient.SendAsync(
                requestMessage,
                cancellationToken);

        return await CreateResponseAsync(
            response,
            cancellationToken);
    }

    public async Task<FargoHttpResponse<TResponse>> DeleteAsync<TResponse>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await httpClient.DeleteAsync(
                uri,
                cancellationToken);

        return await CreateResponseAsync<TResponse>(
            response,
            cancellationToken);
    }

    public async Task<FargoHttpResponse> DeleteAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await httpClient.DeleteAsync(
                uri,
                cancellationToken);

        return await CreateResponseAsync(
            response,
            cancellationToken);
    }

    private async Task<FargoHttpResponse<TResponse>> CreateResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new FargoHttpResponse<TResponse>(
                default,
                await ReadProblemDetailsAsync(
                    response,
                    cancellationToken),
                response);
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new FargoHttpResponse<TResponse>(
                default,
                null,
                response);
        }

        TResponse? value =
            await response.Content.ReadFromJsonAsync<TResponse>(
                jsonSerializerOptions,
                cancellationToken);

        return new FargoHttpResponse<TResponse>(
            value,
            null,
            response);
    }

    private async Task<FargoHttpResponse> CreateResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new FargoHttpResponse(
                await ReadProblemDetailsAsync(
                    response,
                    cancellationToken),
                response);
        }

        return new FargoHttpResponse(
            null,
            response);
    }

    private async Task<ProblemDetails?> ReadProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        return await response.Content.ReadFromJsonAsync<ProblemDetails>(
            jsonSerializerOptions,
            cancellationToken);
    }
}
