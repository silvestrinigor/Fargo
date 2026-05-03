using Fargo.Api.Articles;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Api.Http;

/// <summary>
/// Default implementation of <see cref="IFargoHttpClient"/>. Wraps an <see cref="HttpClient"/>,
/// resolves the request URL against <see cref="FargoSdkOptions.Server"/>, and attaches the
/// <c>X-Api-Key</c> and bearer token from <see cref="FargoSdkOptions"/> per request.
/// </summary>
public sealed class FargoHttpClient : IFargoHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web)
    {
        Converters = { new MassJsonConverter(), new LengthJsonConverter(), new DensityJsonConverter() }
    };

    private static readonly Action<ILogger, string, string, Exception?> RequestLog =
        LoggerMessage.Define<string, string>(LogLevel.Debug, default, "{Method} {Url}");

    private static readonly Action<ILogger, string, string, int, Exception?> ResponseLog =
        LoggerMessage.Define<string, string, int>(LogLevel.Debug, default, "{Method} {Url} -> {StatusCode}");

    private readonly HttpClient httpClient;
    private readonly ILogger logger;
    private readonly FargoSdkOptions options;

    public FargoHttpClient(
        HttpClient httpClient,
        ILogger<FargoHttpClient> logger,
        FargoSdkOptions options)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.options = options;
    }

    public async Task<FargoSdkHttpResponse<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "GET", url, null);

        using var request = BuildRequest(HttpMethod.Get, url);
        using var response = await SendAsync(() => httpClient.SendAsync(request, ct));
        ResponseLog(logger, "GET", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: false,
                Data: default,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: true,
                Data: default,
                Problem: null,
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<TResponse>(
            IsSuccess: true,
            Data: await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct),
            Problem: null,
            StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<TResponse>> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "POST", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Post, url, request);
        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "POST", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<TResponse>(
                IsSuccess: false,
                Data: default,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<TResponse>(
            IsSuccess: true,
            Data: await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct),
            Problem: null,
            StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "POST", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Post, url, request);
        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "POST", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<EmptyResult>(IsSuccess: true, Data: null, Problem: null, StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "PATCH", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Patch, url, request);
        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "PATCH", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<EmptyResult>(IsSuccess: true, Data: null, Problem: null, StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "PUT", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Put, url, request);
        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "PUT", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<EmptyResult>(IsSuccess: true, Data: null, Problem: null, StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> DeleteAsync(string path, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "DELETE", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Delete, url);
        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "DELETE", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<EmptyResult>(IsSuccess: true, Data: null, Problem: null, StatusCode: response.StatusCode);
    }

    public async Task<FargoSdkHttpResponse<EmptyResult>> PutMultipartAsync(
        string path,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "PUT", url, null);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        using var requestMessage = BuildRequest(HttpMethod.Put, url);
        requestMessage.Content = content;

        using var response = await SendAsync(() => httpClient.SendAsync(requestMessage, ct));
        ResponseLog(logger, "PUT", url, (int)response.StatusCode, null);

        if (!response.IsSuccessStatusCode)
        {
            return new FargoSdkHttpResponse<EmptyResult>(
                IsSuccess: false,
                Data: null,
                Problem: await ReadProblemAsync(response.Content, ct),
                StatusCode: response.StatusCode);
        }

        return new FargoSdkHttpResponse<EmptyResult>(IsSuccess: true, Data: null, Problem: null, StatusCode: response.StatusCode);
    }

    public async Task<(Stream Stream, string ContentType)?> GetStreamAsync(string path, CancellationToken ct = default)
    {
        var url = ResolveUrl(path);
        RequestLog(logger, "GET", url, null);

        using var requestMessage = BuildRequest(HttpMethod.Get, url);
        var response = await SendAsync(() => httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, ct));
        ResponseLog(logger, "GET", url, (int)response.StatusCode, null);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            response.Dispose();
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var problem = await ReadProblemAsync(response.Content, ct);
            response.Dispose();
            throw new FargoSdkApiException(FargoSdkProblemMapper.Map(problem));
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

    private HttpRequestMessage BuildRequest(HttpMethod method, string url)
    {
        var message = new HttpRequestMessage(method, url);
        ApplyHeaders(message);
        return message;
    }

    private HttpRequestMessage BuildRequest<TRequest>(HttpMethod method, string url, TRequest body)
    {
        var message = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        ApplyHeaders(message);
        return message;
    }

    private void ApplyHeaders(HttpRequestMessage message)
    {
        if (!string.IsNullOrEmpty(options.AccessToken))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
        }

        if (!string.IsNullOrEmpty(options.ApiKey))
        {
            message.Headers.Add("X-Api-Key", options.ApiKey);
        }
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

    private static async Task<FargoProblemDetails?> ReadProblemAsync(HttpContent content, CancellationToken ct)
    {
        var json = await content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<FargoProblemDetails>(json, JsonOptions);
    }

    private string ResolveUrl(string path) =>
        !string.IsNullOrEmpty(options.Server)
            ? options.Server.TrimEnd('/') + path
            : throw new InvalidOperationException("Server URL is not configured. Set FargoSdkOptions.Server before making requests.");
}
