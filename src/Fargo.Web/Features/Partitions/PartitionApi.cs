using Fargo.Application.Commands.PartitionCommands;
using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;

namespace Fargo.Web.Features.Partitions;

public sealed class PartitionApi(IPartitionClient partitionClient)
{
    public async Task<IReadOnlyList<PartitionSummary>> GetChildrenAsync(
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        var partitions = await partitionClient.GetManyAsync(
            parentPartitionGuid: parentPartitionGuid,
            cancellationToken: cancellationToken);

        return [.. partitions.Select(ToSummary)];
    }

    public async Task<PartitionSummary?> GetSingleAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var partition = await partitionClient.GetSingleAsync(
            partitionGuid,
            cancellationToken: cancellationToken);

        return partition is null ? null : ToSummary(partition);
    }

    public Task<Guid> CreateAsync(
        string name,
        string? description,
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        var command = new PartitionCreateCommand(
            new Name(name),
            string.IsNullOrWhiteSpace(description) ? null : new Description(description),
            parentPartitionGuid);

        return partitionClient.CreateAsync(command, cancellationToken);
    }

    public Task UpdateAsync(
        Guid partitionGuid,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var model = new PartitionUpdateModel(
            string.IsNullOrWhiteSpace(description) ? null : new Description(description));

        return partitionClient.UpdateAsync(partitionGuid, model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default) =>
        partitionClient.DeleteAsync(partitionGuid, cancellationToken);

    private static PartitionSummary ToSummary(PartitionInformation partition) =>
        new(
            partition.Guid,
            partition.Name.Value,
            partition.Description.Value,
            partition.ParentPartitionGuid,
            partition.IsActive
        );
}
