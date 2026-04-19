using Fargo.Domain.Partitions;

namespace Fargo.Domain.Users;

/// <summary>
/// Defines the repository contract for managing <see cref="User"/> entities.
/// </summary>
/// <remarks>
/// This repository provides persistence access and domain queries related to
/// <see cref="User"/> aggregates. It exposes both aggregate retrieval methods
/// and lightweight projection queries used for read operations.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<User?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a user by their unique <see cref="Nameid"/>.
    /// </summary>
    /// <param name="nameid">The unique user identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<User?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified identifier exists.
    /// </summary>
    /// <param name="guid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if a user with the specified identifier exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByGuid(
        Guid guid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    /// <param name="nameid">The unique user identifier to check.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if a user with the specified <see cref="Nameid"/> exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets lightweight information about a user by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the user.</param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned information represents the state of the user
    /// as it existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="UserInformation"/> projection if the user exists;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<UserInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of user information projections.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the users
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="UserInformation"/> objects.
    /// </returns>
    Task<IReadOnlyCollection<UserInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets lightweight information about a user by its unique identifier,
    /// only if the user belongs to at least one of the specified partitions.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the user.</param>
    /// <param name="partitionGuids">
    /// The partitions used to filter access to the user.
    /// The user must belong to at least one of these partitions to be returned.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned information represents the state of the user
    /// as it existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="UserInformation"/> projection if the user exists and belongs
    /// to at least one of the specified partitions; otherwise, <see langword="null"/>.
    /// </returns>
    Task<UserInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of user information projections,
    /// filtered to users that belong to at least one of the specified partitions.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="partitionGuids">
    /// The partitions used to filter accessible users.
    /// Only users belonging to at least one of these partitions are returned.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the users
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="UserInformation"/> objects.
    /// </returns>
    Task<IReadOnlyCollection<UserInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of user unique identifiers.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the users
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of user unique identifiers.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of user unique identifiers,
    /// filtered to users that belong to at least one of the specified partitions.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="partitionGuids">
    /// The partitions used to filter accessible users.
    /// Only users belonging to at least one of these partitions are returned.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the users
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of user unique identifiers.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the partitions that directly contain the specified user.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the user.</param>
    /// <param name="partitionFilter">
    /// When provided, only partitions within this set are returned.
    /// Pass <see langword="null"/> to return all partitions (admin/system path).
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="PartitionInformation"/> representing the
    /// partitions that directly contain the user; or <see langword="null"/> if
    /// the user does not exist.
    /// </returns>
    Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new user to the persistence context.
    /// </summary>
    /// <param name="user">The user to add.</param>
    void Add(User user);

    /// <summary>
    /// Removes a user from the persistence context.
    /// </summary>
    /// <param name="user">The user to remove.</param>
    void Remove(User user);

    /// <summary>
    /// Determines whether any users exist in the system.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if at least one user exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> Any(CancellationToken cancellationToken = default);
}
