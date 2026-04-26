namespace Fargo.Sdk.UserGroups;

/// <summary>Delegate façade that implements <see cref="IUserGroupManager"/> by composing the focused services.</summary>
public sealed class UserGroupManager(IUserGroupService service, IUserGroupEventSource eventSource) : IUserGroupManager
{
    /// <inheritdoc />
    public event EventHandler<UserGroupCreatedEventArgs>? Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    /// <inheritdoc />
    public Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => service.GetAsync(userGroupGuid, temporalAsOf, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<UserGroup>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default)
        => service.GetManyAsync(userGuid, temporalAsOf, page, limit, cancellationToken);

    /// <inheritdoc />
    public Task<UserGroup> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
        => service.CreateAsync(nameid, description, permissions, firstPartition, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(userGroupGuid, cancellationToken);
}
