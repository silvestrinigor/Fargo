using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Items;

/// <summary>Low-level HTTP transport for item endpoints.</summary>
public interface IItemHttpClient
{
    Task<FargoSdkResponse<ItemResult>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
