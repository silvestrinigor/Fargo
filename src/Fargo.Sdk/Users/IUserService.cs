namespace Fargo.Sdk.Users;

/// <summary>Provides CRUD operations for users and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IUserService
{
    Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    Task<User> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
