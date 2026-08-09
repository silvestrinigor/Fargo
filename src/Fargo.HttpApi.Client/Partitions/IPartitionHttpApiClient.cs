using Fargo.Application;
using Fargo.Application.Shared.Partitions;

namespace Fargo.HttpApi.Client.Partitions;

public interface IPartitionHttpApiClient
{
    Task<PartitionDto?> GetAsync(Guid partitionGuid, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PartitionDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(PartitionCreateDto request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid partitionGuid, PartitionUpdateDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
