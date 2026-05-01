namespace Fargo.Api.ApiClients;

/// <summary>Represents a live API client entity.</summary>
public sealed class ApiClient : IAsyncDisposable
{
    internal ApiClient(Guid guid, string name, string description, bool isActive, IApiClientHttpClient client, Func<ValueTask>? onDispose = null, Guid? editedByGuid = null)
    {
        Guid = guid;
        Name = name;
        Description = description;
        IsActive = isActive;
        EditedByGuid = editedByGuid;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IApiClientHttpClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>Gets the unique identifier of the API client.</summary>
    public Guid Guid { get; }

    /// <summary>Gets or sets the display name of the API client.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the description of the API client.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the API client is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets the unique identifier of the user who last modified this API client.</summary>
    public Guid? EditedByGuid { get; }

    /// <summary>Raised when this API client is updated by any authenticated client.</summary>
    public event EventHandler<ApiClientUpdatedEventArgs>? Updated;

    /// <summary>Raised when this API client is deleted by any authenticated client.</summary>
    public event EventHandler<ApiClientDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ApiClientUpdatedEventArgs(Guid));
    internal void RaiseDeleted() => Deleted?.Invoke(this, new ApiClientDeletedEventArgs(Guid));

    /// <summary>Applies <paramref name="configure"/> to this entity and persists the changes.</summary>
    /// <param name="configure">A delegate that mutates the entity properties.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Action<ApiClient> configure, CancellationToken cancellationToken = default)
    {
        configure(this);
        return client.UpdateAsync(Guid, Name, Description, IsActive, cancellationToken);
    }

    /// <summary>Deletes this API client.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public Task<FargoSdkResponse<EmptyResult>> DeleteAsync(CancellationToken cancellationToken = default)
        => client.DeleteAsync(Guid, cancellationToken);

    /// <summary>Removes this entity from the local tracking dictionary.</summary>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
