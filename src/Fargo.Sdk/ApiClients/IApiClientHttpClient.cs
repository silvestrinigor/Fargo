namespace Fargo.Sdk.ApiClients;

/// <summary>Low-level HTTP transport for API client endpoints.</summary>
public interface IApiClientHttpClient
{
    Task<FargoSdkResponse<ApiClientResult>> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
    Task<FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default);
    Task<FargoSdkResponse<ApiClientCreatedResult>> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid apiClientGuid, string? name, string? description, bool? isActive, CancellationToken cancellationToken = default);
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
}
