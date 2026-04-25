using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Represents a live user group entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// Dispose to unsubscribe from real-time events.
/// </summary>
public sealed class UserGroup : IAsyncDisposable
{
    internal UserGroup(
        Guid guid,
        string nameid,
        string description,
        bool isActive,
        IReadOnlyCollection<ActionType> permissions,
        IUserGroupHttpClient client,
        Func<ValueTask>? onDispose = null)
    {
        Guid = guid;
        _nameid = nameid;
        _description = description;
        _isActive = isActive;
        _permissions = permissions;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IUserGroupHttpClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>The unique identifier of the user group.</summary>
    public Guid Guid { get; }

    private string _nameid;

    /// <summary>The name identifier of the user group.</summary>
    public string Nameid
    {
        get => _nameid;
        set => _nameid = value;
    }

    private string _description;

    /// <summary>The description of the user group.</summary>
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    private bool _isActive;

    /// <summary>Whether the user group is active.</summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    /// <summary>Raised when this user group is updated by any authenticated client.</summary>
    public event EventHandler<UserGroupUpdatedEventArgs>? Updated;

    /// <summary>Raised when this user group is deleted by any authenticated client.</summary>
    public event EventHandler<UserGroupDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new UserGroupUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new UserGroupDeletedEventArgs(Guid));

    private IReadOnlyCollection<ActionType> _permissions;

    /// <summary>The permissions assigned to this user group.</summary>
    public IReadOnlyCollection<ActionType> Permissions
    {
        get => _permissions;
        set => _permissions = value;
    }

    /// <summary>Gets the partitions that directly contain this user group.</summary>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    /// <summary>
    /// Applies <paramref name="update"/> to this user group and persists all changes in a single request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this user group.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<UserGroup> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var result = await client.UpdateAsync(Guid, _nameid, _description, _isActive, _permissions, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
