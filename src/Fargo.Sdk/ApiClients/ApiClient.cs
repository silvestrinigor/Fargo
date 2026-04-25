namespace Fargo.Sdk.ApiClients;

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

    public Guid Guid { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid? EditedByGuid { get; }

    public event EventHandler<ApiClientUpdatedEventArgs>? Updated;
    public event EventHandler<ApiClientDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ApiClientUpdatedEventArgs(Guid));
    internal void RaiseDeleted() => Deleted?.Invoke(this, new ApiClientDeletedEventArgs(Guid));

    public Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Action<ApiClient> configure, CancellationToken cancellationToken = default)
    {
        configure(this);
        return client.UpdateAsync(Guid, Name, Description, IsActive, cancellationToken);
    }

    public Task<FargoSdkResponse<EmptyResult>> DeleteAsync(CancellationToken cancellationToken = default)
        => client.DeleteAsync(Guid, cancellationToken);

    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
