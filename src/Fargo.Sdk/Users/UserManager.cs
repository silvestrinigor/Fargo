namespace Fargo.Api.Users;

/// <summary>Delegate façade that implements <see cref="IUserManager"/> by composing the focused services.</summary>
public sealed class UserManager(IUserService service, IUserEventSource eventSource) : IUserManager
{
    /// <inheritdoc />
    public event EventHandler<UserCreatedEventArgs>? Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    /// <inheritdoc />
    public Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => service.GetAsync(userGuid, temporalAsOf, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<User>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default)
        => service.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken);

    /// <inheritdoc />
    public Task<User> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
        => service.CreateAsync(nameid, password, firstName, lastName, description, permissions, defaultPasswordExpirationPeriod, firstPartition, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(userGuid, cancellationToken);
}
