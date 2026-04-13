using Fargo.Sdk.Authentication;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Sdk.Http;

public sealed class FargoSdkHttpClient : IFargoSdkHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient httpClient;

    private readonly AuthSession session;

    private readonly ILogger logger;

    private string? baseUrl;

    public FargoSdkHttpClient(HttpClient httpClient, AuthSession session, ILogger logger)
    {
        this.httpClient = httpClient;
        this.session = session;
        this.logger = logger;
    }

    internal void SetBaseUrl(string url) => baseUrl = url.TrimEnd('/');

    public async Task<FargoSdkHttpResponse<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("GET", url);

        using var response = await SendAsync(() => httpClient.GetAsync(url, ct));
        logger.LogResponse("GET", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: false,
                Data: default,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: true,
                Data: default,
                Problem: null,
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<TResponse>(
            IsSuccess: true,
            Data: await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct),
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<TResponse>> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("POST", url);

        using var response = await SendAsync(() => httpClient.PostAsJsonAsync(url, request, JsonOptions, ct));
        logger.LogResponse("POST", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: false,
                Data: default,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<TResponse>(
            IsSuccess: true,
            Data: await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct),
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("POST", url);

        using var response = await SendAsync(() => httpClient.PostAsJsonAsync(url, request, JsonOptions, ct));
        logger.LogResponse("POST", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<EmptyResult>(
            IsSuccess: true,
            Data: null,
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("PATCH", url);

        using var response = await SendAsync(() => httpClient.PatchAsJsonAsync(url, request, JsonOptions, ct));
        logger.LogResponse("PATCH", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<EmptyResult>(
            IsSuccess: true,
            Data: null,
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("PUT", url);

        using var response = await SendAsync(() => httpClient.PutAsJsonAsync(url, request, JsonOptions, ct));
        logger.LogResponse("PUT", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<EmptyResult>(
            IsSuccess: true,
            Data: null,
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> DeleteAsync(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("DELETE", url);

        using var response = await SendAsync(() => httpClient.DeleteAsync(url, ct));
        logger.LogResponse("DELETE", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<EmptyResult>(
            IsSuccess: true,
            Data: null,
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PutMultipartAsync(
        string path,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("PUT", url);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        using var response = await SendAsync(() => httpClient.PutAsync(url, content, ct));
        logger.LogResponse("PUT", url, (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct),
                StatusCode: response.StatusCode
            );
        }

        return new FargoSdkHttpResponse<EmptyResult>(
            IsSuccess: true,
            Data: null,
            Problem: null,
            StatusCode: response.StatusCode
        );
    }

    public async Task<(Stream Stream, string ContentType)?> GetStreamAsync(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("GET", url);

        var response = await SendAsync(() => httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct));
        logger.LogResponse("GET", url, (int)response.StatusCode);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            response.Dispose();
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content.ReadFromJsonAsync<FargoProblemDetails>(JsonOptions, ct);
            response.Dispose();
            throw new FargoSdkApiException(problem?.Detail ?? "An unexpected error occurred.");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var stream = await response.Content.ReadAsStreamAsync(ct);

        return (stream, contentType);
    }

    public static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var parts = parameters
            .Where(p => p.Value is not null)
            .Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value!));

        var query = string.Join("&", parts);

        return query.Length > 0 ? "?" + query : string.Empty;
    }

    private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException ex)
        {
            throw new FargoSdkConnectionException(ex.Message, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new FargoSdkConnectionException("The request timed out.", ex);
        }
    }

    private string ResolveUrl(string path) =>
        baseUrl is not null
            ? baseUrl + path
            : throw new InvalidOperationException("Server URL is not configured. Call engine.LogInAsync(server, ...) or engine.RestoreSessionAsync(server, ...) first.");

    private void ApplyAuth()
    {
        httpClient.DefaultRequestHeaders.Authorization = session.IsAuthenticated
            ? new AuthenticationHeaderValue("Bearer", session.AccessToken)
            : null;
    }
}
