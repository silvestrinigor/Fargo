using Fargo.Application.Commands.PartitionCommands;
using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class PartitionClient(HttpClient http)
    : FargoHttpClientBase(http), IPartitionClient
{
    public Task<PartitionInformation?> GetSingleAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/partitions/{partitionGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<PartitionInformation?>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<PartitionInformation>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/partitions?parentPartitionGuid={parentPartitionGuid}&temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<PartitionInformation>(uri, cancellationToken);
    }

    public Task<Guid> CreateAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<Guid>("/partitions", command, cancellationToken);
    }

    public Task UpdateAsync(
        Guid partitionGuid,
        PartitionUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync($"/partitions/{partitionGuid}", model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);
    }
}
