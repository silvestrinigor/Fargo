using Fargo.Application;
using Fargo.Application.Shared.Partitions;

namespace Fargo.HttpApi.Client.Partitions;

public sealed class PartitionHttpApiClient : IPartitionHttpApiClient
{
    public Task<Guid> CreateAsync(PartitionCreateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PartitionDto?> GetAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<PartitionDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid partitionGuid, PartitionUpdateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
