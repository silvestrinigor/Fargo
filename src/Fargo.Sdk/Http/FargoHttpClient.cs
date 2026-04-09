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

    private string baseUrl = string.Empty;

    public FargoHttpClient(HttpClient httpClient, AuthSession session)
    {
        this.httpClient = httpClient;
        this.session = session;
    }

    public void SetBaseUrl(string url)
    {
        baseUrl = url.TrimEnd('/');
    }

    public async Task<TResponse?> GetFromJsonAsync<TResponse>(string path, CancellationToken ct = default)
        where TResponse : class
    {
        ApplyAuth();

        using var response = await httpClient.GetAsync(baseUrl + path, ct);

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

        using var response = await httpClient.PostAsJsonAsync(baseUrl + path, request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);

        return result!;
    }

    public async Task PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PostAsJsonAsync(baseUrl + path, request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PatchAsJsonAsync(baseUrl + path, request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.PutAsJsonAsync(baseUrl + path, request, JsonOptions, ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        ApplyAuth();

        using var response = await httpClient.DeleteAsync(baseUrl + path, ct);

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
            throw new Exception();
        }

        if (statusCode == 404)
        {
            throw new Exception();
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        throw new Exception();
    }
}
