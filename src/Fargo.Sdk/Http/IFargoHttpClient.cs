namespace Fargo.Api.Http;

/// <summary>Low-level HTTP transport for the Fargo API. Handles auth headers and error mapping.</summary>
public interface IFargoHttpClient
{
    /// <summary>Sends a GET request and deserializes the JSON response body.</summary>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default);

    /// <summary>Sends a POST request with a JSON body and deserializes the JSON response body.</summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="request">The request body to serialize.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<TResponse>> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default);

    /// <summary>Sends a POST request with a JSON body and returns an empty result.</summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="request">The request body to serialize.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<EmptyResult>> PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    /// <summary>Sends a PATCH request with a JSON body and returns an empty result.</summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="request">The request body to serialize.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<EmptyResult>> PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    /// <summary>Sends a PUT request with a JSON body and returns an empty result.</summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="request">The request body to serialize.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<EmptyResult>> PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    /// <summary>Sends a DELETE request and returns an empty result.</summary>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<FargoSdkHttpResponse<EmptyResult>> DeleteAsync(string path, CancellationToken ct = default);

    /// <summary>Sends a multipart/form-data PUT request with a single file field named <c>file</c>.</summary>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="stream">The file data stream.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="fileName">The file name hint.</param>
    /// <param name="ct">A token to cancel the operation.</param>
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
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<(Stream Stream, string ContentType)?> GetStreamAsync(string path, CancellationToken ct = default);
}
