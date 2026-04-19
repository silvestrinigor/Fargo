using Fargo.Sdk.Http;
using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Items;

public sealed class ItemClient : IItemClient
{
    internal ItemClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<ItemResult>> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")));

        var httpResponse = await httpClient.GetAsync<ItemResult>(
            $"/items/{itemGuid}{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ItemResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ItemResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("articleGuid", articleGuid?.ToString()),
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("partitionGuid", partitionGuid?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ItemResult>>(
            $"/items{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ItemResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ItemResult>>(httpResponse.Data ?? []);
    }

    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        Guid articleGuid,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, Guid>(
            "/items",
            new
            {
                item = new
                {
                    articleGuid,
                    firstPartition
                }
            },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync(
            $"/items/{itemGuid}",
            new { },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync(
            $"/items/{itemGuid}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid itemGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync(
            $"/items/{itemGuid}/partitions/{partitionGuid}",
            new { },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid itemGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync(
            $"/items/{itemGuid}/partitions/{partitionGuid}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionResult>>(
            $"/items/{itemGuid}/partitions",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(httpResponse.Data ?? []);
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem)
    {
        var type = problem?.Type switch
        {
            "item/not-found"
                or "article/not-found" => FargoSdkErrorType.NotFound,
            "auth/unauthorized" => FargoSdkErrorType.UnauthorizedAccess,
            "user/forbidden"
                or "partition/access-denied"
                or "entity/access-denied" => FargoSdkErrorType.Forbidden,
            "request/invalid" => FargoSdkErrorType.InvalidInput,
            _ => FargoSdkErrorType.Undefined
        };

        var detail = problem?.Detail ?? "An unexpected error occurred.";

        return new FargoSdkError(type, detail);
    }
}
