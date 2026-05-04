using Fargo.Api.Http;
using Fargo.Api.Partitions;
using Fargo.Sdk;
using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;

namespace Fargo.Api.Items;

/// <summary>Default implementation of <see cref="IItemHttpClient"/>.</summary>
public sealed class ItemHttpClient : IItemHttpClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ItemHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ItemResult>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ItemDto>($"/items/{itemGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ItemResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ItemResult>(httpResponse.Data!.ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("articleGuid", articleGuid?.ToString()),
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("partitionGuid", partitionGuid?.ToString()),
            ("noPartition", noPartition?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ItemDto>>($"/items{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ItemResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ItemResult>>((httpResponse.Data ?? []).ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ItemCreateDto, Guid>(
            "/items",
            ContractMappings.ToItemCreateDto(articleGuid, firstPartition, productionDate),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid itemGuid, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync<ItemUpdateDto>(
            $"/items/{itemGuid}",
            ContractMappings.ToItemUpdateDto(productionDate),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/items/{itemGuid}/partitions/{partitionGuid}", new { }, cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}/partitions/{partitionGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionDto>>($"/items/{itemGuid}/partitions", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>((httpResponse.Data ?? []).ToSdk());
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
