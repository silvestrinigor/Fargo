using Fargo.Sdk.Authentication;
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

    private string? baseUrl;

    public FargoHttpClient(HttpClient httpClient, AuthSession session)
    {
        this.httpClient = httpClient;
        this.session = session;
    }

    internal void SetBaseUrl(string url) => baseUrl = url.TrimEnd('/');

    public async Task<TResponse?> GetFromJsonAsync<TResponse>(string path, CancellationToken ct = default)
        where TResponse : class
    {
        ApplyAuth();

        using var response = await httpClient.GetAsync(ResolveUrl(path), ct);

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

        using var response = await httpClient.PostAsJsonAsync(ResolveUrl(path), request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);

        return result!;
    }

    public async Task PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PostAsJsonAsync(ResolveUrl(path), request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PatchAsJsonAsync(ResolveUrl(path), request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PutAsJsonAsync(ResolveUrl(path), request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.DeleteAsync(ResolveUrl(path), ct);

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

        throw new FargoHttpException(statusCode, content);
    }
}
