namespace Fargo.Sdk.Partitions;

/// <summary>Low-level HTTP transport for partition endpoints.</summary>
public interface IPartitionHttpClient
{
    Task<FargoSdkResponse<PartitionResult>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid partitionGuid, string? name = null, string? description = null, Guid? parentPartitionGuid = null, bool? isActive = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
