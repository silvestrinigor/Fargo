using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

/// <summary>Persistence contract for write-side API client operations.</summary>
public interface IApiClientRepository
{
    /// <summary>Adds a new API client to the repository.</summary>
    /// <param name="client">The API client entity to add.</param>
    void Add(ApiClient client);

    /// <summary>Marks an API client for deletion.</summary>
    /// <param name="client">The API client entity to remove.</param>
    void Remove(ApiClient client);

    /// <summary>Returns the API client with the given identifier, or <see langword="null"/> if not found.</summary>
    /// <param name="guid">The unique identifier to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<ApiClient?> GetByGuid(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Returns <see langword="true"/> if an API client with the given identifier exists.</summary>
    /// <param name="guid">The unique identifier to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsByGuid(Guid guid, CancellationToken cancellationToken = default);
}
