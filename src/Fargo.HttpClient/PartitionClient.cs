using Fargo.Application.Shared.Partitions;

namespace Fargo.HttpClient;

public interface IFargoPartitionClient
{
    Task<PartitionDto?> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PartitionDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        PartitionCreateDto request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid partitionGuid,
        PartitionUpdateDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoPartitionClient(FargoHttpTransport transport) : IFargoPartitionClient
{
    public Task<PartitionDto?> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<PartitionDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/partitions/{partitionGuid:D}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<IReadOnlyCollection<PartitionDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default)
        => transport.SendCollectionAsync<PartitionDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath("/partitions/", FargoHttpTransport.ListQuery(query)),
            null,
            cancellationToken);

    public Task<Guid> CreateAsync(
        PartitionCreateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<Guid>(
            HttpMethod.Post,
            "/partitions/",
            request,
            cancellationToken);

    public Task UpdateAsync(
        Guid partitionGuid,
        PartitionUpdateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            $"/partitions/{partitionGuid:D}",
            request,
            cancellationToken);

    public Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Delete,
            $"/partitions/{partitionGuid:D}",
            null,
            cancellationToken);
}
