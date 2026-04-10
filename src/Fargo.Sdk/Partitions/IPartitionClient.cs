namespace Fargo.Sdk.Partitions;

public interface IPartitionClient
{
    Task<FargoSdkResponse<PartitionResult>> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid partitionGuid,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default);
}
