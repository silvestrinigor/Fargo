using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Actors.Users;

public interface IUserRepository
{
    Task<User?> GetByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default);

    Task<User?> GetByNameidAsync(Nameid userNameid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Partitions the user can access, granted directly or through active user groups and their
    /// parent groups. Empty when the user is inactive; inactive groups contribute nothing.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsFromUserAsync(
        Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permissions granted directly or through active user groups and their parent groups. Empty
    /// when the user is inactive; inactive groups contribute nothing.
    /// </summary>
    Task<IReadOnlyCollection<ActionType>> GetAllActivePermissionsFromUserAsync(
        Guid userGuid, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    void Add(User user);

    void Remove(User user);
}
