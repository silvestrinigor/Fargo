using Fargo.Sdk.Http;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionHttpClient"/>.</summary>
public sealed class PartitionHttpClient : IPartitionHttpClient
{
    /// <summary>Initializes a new instance.</summary>
    public PartitionHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<PartitionResult>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<PartitionResult>($"/partitions/{partitionGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<PartitionResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<PartitionResult>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("parentPartitionGuid", parentPartitionGuid?.ToString()),
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("rootOnly", rootOnly?.ToString().ToLowerInvariant()),
            ("search", search));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionResult>>($"/partitions{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid partitionGuid, string? name = null, string? description = null, Guid? parentPartitionGuid = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync(
            $"/partitions/{partitionGuid}",
            new { name, description, parentPartitionGuid, isActive },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);

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
