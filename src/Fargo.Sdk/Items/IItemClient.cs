namespace Fargo.Sdk.Items;

public interface IItemClient
{
    Task<FargoSdkResponse<ItemResult>> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(
        Guid articleGuid,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);
}
