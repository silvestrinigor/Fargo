namespace Fargo.Sdk.Http;

/// <summary>Low-level HTTP transport for the Fargo API. Handles auth headers and error mapping.</summary>
public interface IFargoHttpClient
{
    Task<FargoSdkHttpResponse<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<TResponse>> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> DeleteAsync(string path, CancellationToken ct = default);

    /// <summary>Sends a multipart/form-data PUT request with a single file field named <c>file</c>.</summary>
    Task<FargoSdkHttpResponse<EmptyResult>> PutMultipartAsync(
        string path,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a GET request and returns the raw response stream with its content type.
    /// Returns <see langword="null"/> on a 404; throws on other non-success status codes.
    /// </summary>
    Task<(Stream Stream, string ContentType)?> GetStreamAsync(string path, CancellationToken ct = default);
}
