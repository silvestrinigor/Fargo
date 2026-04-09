using Fargo.Sdk.Authentication;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Sdk.Http;

public sealed class FargoHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient httpClient;

    private readonly AuthSession session;

    private readonly ILogger logger;

    private string? baseUrl;

    public FargoHttpClient(HttpClient httpClient, AuthSession session, ILogger logger)
    {
        this.httpClient = httpClient;
        this.session = session;
        this.logger = logger;
    }

    internal void SetBaseUrl(string url) => baseUrl = url.TrimEnd('/');

    public async Task<TResponse?> GetFromJsonAsync<TResponse>(string path, CancellationToken ct = default)
        where TResponse : class
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("GET", url);

        using var response = await httpClient.GetAsync(url, ct);
        logger.LogResponse("GET", url, (int)response.StatusCode);

        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.NoContent)
        {
            return null;
        }

        await EnsureSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
    }

    public async Task<TResponse> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("POST", url);

        using var response = await httpClient.PostAsJsonAsync(url, request, JsonOptions, ct);
        logger.LogResponse("POST", url, (int)response.StatusCode);

        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);

        return result!;
    }

    public async Task PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("POST", url);

        using var response = await httpClient.PostAsJsonAsync(url, request, JsonOptions, ct);
        logger.LogResponse("POST", url, (int)response.StatusCode);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("PATCH", url);

        using var response = await httpClient.PatchAsJsonAsync(url, request, JsonOptions, ct);
        logger.LogResponse("PATCH", url, (int)response.StatusCode);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("PUT", url);

        using var response = await httpClient.PutAsJsonAsync(url, request, JsonOptions, ct);
        logger.LogResponse("PUT", url, (int)response.StatusCode);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        var url = ResolveUrl(path);
        logger.LogRequest("DELETE", url);

        using var response = await httpClient.DeleteAsync(url, ct);
        logger.LogResponse("DELETE", url, (int)response.StatusCode);

        await EnsureSuccessAsync(response, ct);
    }

    public static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var parts = parameters
            .Where(p => p.Value is not null)
            .Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value!));

        var query = string.Join("&", parts);

        return query.Length > 0 ? "?" + query : string.Empty;
    }

    private string ResolveUrl(string path) =>
        baseUrl is not null
            ? baseUrl + path
            : throw new InvalidOperationException("Server URL is not configured. Set it via engine.Server.SetUrlAsync() before making requests.");

    private void ApplyAuth()
    {
        httpClient.DefaultRequestHeaders.Authorization = session.IsAuthenticated
            ? new AuthenticationHeaderValue("Bearer", session.AccessToken)
            : null;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var statusCode = (int)response.StatusCode;

        if (statusCode == 401)
        {
            throw new FargoUnauthorizedException();
        }
        if (statusCode == 404)
        {
            throw new FargoNotFoundException();
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        throw new FargoSdkHttpException(statusCode, content);
    }
}
