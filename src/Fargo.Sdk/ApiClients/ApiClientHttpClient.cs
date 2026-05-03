using Fargo.Sdk.Contracts.ApiClients;
using Fargo.Api.Http;
using Fargo.Sdk;

namespace Fargo.Api.ApiClients;

/// <summary>Default implementation of <see cref="IApiClientHttpClient"/>.</summary>
public sealed class ApiClientHttpClient : IApiClientHttpClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ApiClientHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ApiClientResult>> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var r = await httpClient.GetAsync<ApiClientDto>($"/api-clients/{apiClientGuid}", cancellationToken);
        return r.IsSuccess ? new(r.Data!.ToSdk()) : new(MapError(r.Problem));
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("search", search));
        var r = await httpClient.GetAsync<IReadOnlyCollection<ApiClientDto>>($"/api-clients{query}", cancellationToken);
        return r.IsSuccess ? new((r.Data ?? []).ToSdk()) : new(MapError(r.Problem));
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ApiClientCreatedDto>> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var r = await httpClient.PostFromJsonAsync<ApiClientCreateDto, ApiClientCreatedDto>(
            "/api-clients",
            ContractMappings.ToApiClientCreateDto(name, description),
            cancellationToken);
        return r.IsSuccess ? new(r.Data!) : new(MapError(r.Problem));
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid apiClientGuid, string? name, string? description, bool? isActive, CancellationToken cancellationToken = default)
    {
        var r = await httpClient.PatchJsonAsync<ApiClientUpdateDto>(
            "/api-clients/" + apiClientGuid,
            ContractMappings.ToApiClientUpdateDto(name, description, isActive),
            cancellationToken);
        return r.IsSuccess ? new(new EmptyResult()) : new(MapError(r.Problem));
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var r = await httpClient.DeleteAsync($"/api-clients/{apiClientGuid}", cancellationToken);
        return r.IsSuccess ? new(new EmptyResult()) : new(MapError(r.Problem));
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
