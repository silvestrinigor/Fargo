using Fargo.Sdk.Http;

namespace Fargo.Sdk.Partitions;

public sealed class PartitionClient : IPartitionClient
{
    internal PartitionClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<PartitionResult>> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")));

        var httpResponse = await httpClient.GetAsync<PartitionResult>(
            $"/partitions/{partitionGuid}{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<PartitionResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<PartitionResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        bool? rootOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("parentPartitionGuid", parentPartitionGuid?.ToString()),
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("rootOnly", rootOnly?.ToString().ToLowerInvariant()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionResult>>(
            $"/partitions{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(httpResponse.Data ?? []);
    }

    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, Guid>(
            "/partitions",
            new { name, description, parentPartitionGuid },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid partitionGuid,
        string? name = null,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync(
            $"/partitions/{partitionGuid}",
            new { name, description, parentPartitionGuid },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync(
            $"/partitions/{partitionGuid}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem)
    {
        var type = problem?.Type switch
        {
            "partition/not-found" => FargoSdkErrorType.NotFound,
            "auth/unauthorized" => FargoSdkErrorType.UnauthorizedAccess,
            "user/forbidden"
                or "partition/access-denied" => FargoSdkErrorType.Forbidden,
            "request/invalid"
                or "partition/circular-hierarchy"
                or "partition/cannot-be-own-parent"
                or "partition/cannot-delete-global" => FargoSdkErrorType.InvalidInput,
            _ => FargoSdkErrorType.Undefined
        };

        var detail = problem?.Detail ?? "An unexpected error occurred.";

        return new FargoSdkError(type, detail);
    }
}
