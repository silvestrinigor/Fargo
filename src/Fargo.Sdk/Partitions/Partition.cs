using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Partitions;

/// <summary>
/// Represents a live partition entity. Setting <see cref="Description"/> automatically
/// sends a PATCH request to the backend to persist the change.
/// Use <see cref="MoveAsync"/> to change the partition's position in the hierarchy.
/// </summary>
public sealed class Partition
{
    internal Partition(
        Guid guid,
        string name,
        string description,
        Guid? parentPartitionGuid,
        bool isActive,
        IPartitionClient client,
        ILogger logger)
    {
        Guid = guid;
        Name = name;
        _description = description;
        ParentPartitionGuid = parentPartitionGuid;
        IsActive = isActive;
        this.client = client;
        this.logger = logger;
    }

    private readonly IPartitionClient client;
    private readonly ILogger logger;

    /// <summary>The unique identifier of the partition.</summary>
    public Guid Guid { get; }

    /// <summary>The display name of the partition.</summary>
    public string Name { get; }

    /// <summary>The unique identifier of the parent partition, or <see langword="null"/> for top-level partitions.</summary>
    public Guid? ParentPartitionGuid { get; }

    /// <summary>Whether the partition is currently active.</summary>
    public bool IsActive { get; }

    /// <summary>Raised when this partition is updated by any authenticated client.</summary>
    public event EventHandler<PartitionUpdatedEventArgs>? Updated;

    /// <summary>Raised when this partition is deleted by any authenticated client.</summary>
    public event EventHandler<PartitionDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new PartitionUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new PartitionDeletedEventArgs(Guid));

    private string _description;

    /// <summary>
    /// The description of the partition. Setting this property fires a background update request.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description == value)
            {
                return;
            }

            _description = value;
            _ = SendUpdateAsync();
        }
    }

    /// <summary>
    /// Moves this partition under a new parent partition.
    /// </summary>
    /// <param name="newParentPartitionGuid">The unique identifier of the new parent partition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public Task<FargoSdkResponse<EmptyResult>> MoveAsync(
        Guid newParentPartitionGuid,
        CancellationToken cancellationToken = default)
        => client.UpdateAsync(Guid, null, newParentPartitionGuid, cancellationToken);

    private async Task SendUpdateAsync()
    {
        var result = await client.UpdateAsync(Guid, _description);

        if (!result.IsSuccess)
        {
            logger.LogPartitionUpdateFailed(Guid, result.Error!.Detail);
        }
    }
}
