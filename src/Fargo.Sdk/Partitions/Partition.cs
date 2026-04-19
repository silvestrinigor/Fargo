namespace Fargo.Sdk.Partitions;

/// <summary>
/// Represents a live partition entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// Use <see cref="MoveAsync"/> to change the partition's position in the hierarchy.
/// Dispose to unsubscribe from real-time events.
/// </summary>
public sealed class Partition : IAsyncDisposable
{
    internal Partition(
        Guid guid,
        string name,
        string description,
        Guid? parentPartitionGuid,
        bool isActive,
        IPartitionClient client,
        Func<ValueTask>? onDispose = null)
    {
        Guid = guid;
        _name = name;
        _description = description;
        ParentPartitionGuid = parentPartitionGuid;
        _isActive = isActive;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IPartitionClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>The unique identifier of the partition.</summary>
    public Guid Guid { get; }

    private string _name;
    private bool _nameChanged;

    /// <summary>The display name of the partition.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                _nameChanged = true;
            }
        }
    }

    /// <summary>The unique identifier of the parent partition, or <see langword="null"/> for top-level partitions.</summary>
    public Guid? ParentPartitionGuid { get; }

    private bool _isActive;
    private bool _isActiveChanged;

    /// <summary>Whether the partition is currently active.</summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                _isActiveChanged = true;
            }
        }
    }

    /// <summary>Raised when this partition is updated by any authenticated client.</summary>
    public event EventHandler<PartitionUpdatedEventArgs>? Updated;

    /// <summary>Raised when this partition is deleted by any authenticated client.</summary>
    public event EventHandler<PartitionDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new PartitionUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new PartitionDeletedEventArgs(Guid));

    private string _description;

    /// <summary>The description of the partition.</summary>
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    /// <summary>
    /// Moves this partition under a new parent partition.
    /// </summary>
    /// <param name="newParentPartitionGuid">The unique identifier of the new parent partition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public Task<FargoSdkResponse<EmptyResult>> MoveAsync(
        Guid newParentPartitionGuid,
        CancellationToken cancellationToken = default)
        => client.UpdateAsync(Guid, null, null, newParentPartitionGuid, null, cancellationToken);

    /// <summary>
    /// Applies <paramref name="update"/> to this partition and persists all changes in a single request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this partition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<Partition> update, CancellationToken cancellationToken = default)
    {
        _nameChanged = false;
        _isActiveChanged = false;
        update(this);
        var nameToSend = _nameChanged ? _name : null;
        var isActiveToSend = _isActiveChanged ? (bool?)_isActive : null;
        _nameChanged = false;
        _isActiveChanged = false;
        var result = await client.UpdateAsync(Guid, nameToSend, _description, null, isActiveToSend, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
