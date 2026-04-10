namespace Fargo.Sdk.Http;

public interface IFargoSdkHttpClient
{
    Task<FargoSdkHttpResponse<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default)
        where TResponse : class;

    Task<FargoSdkHttpResponse<TResponse>> PostFromJsonAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default)
        where TResponse : class;

    Task<FargoSdkHttpResponse<EmptyResult>> PostJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> PatchJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> PutJsonAsync<TRequest>(string path, TRequest request, CancellationToken ct = default);

    Task<FargoSdkHttpResponse<EmptyResult>> DeleteAsync(string path, CancellationToken ct = default);
}
