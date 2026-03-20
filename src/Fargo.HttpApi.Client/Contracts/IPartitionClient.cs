using Fargo.Application.Commands.PartitionCommands;
using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface IPartitionClient
{
    Task<PartitionInformation?> GetSingleAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PartitionInformation>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid partitionGuid,
        PartitionUpdateModel model,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default);
}
