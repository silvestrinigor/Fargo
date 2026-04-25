using Fargo.Domain;

namespace Fargo.Application.ApiClients;

/// <summary>Persistence contract for read-side API client queries.</summary>
public interface IApiClientQueryRepository
{
    /// <summary>Returns the API client information for the given identifier, or <see langword="null"/> if not found.</summary>
    /// <param name="guid">The unique identifier to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<ApiClientInformation?> GetInfoByGuid(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Returns a paged, optionally filtered list of API client information records.</summary>
    /// <param name="pagination">Pagination parameters, or <see langword="null"/> for defaults.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyCollection<ApiClientInformation>> GetManyInfo(Pagination? pagination = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Finds the identifier of an active API client whose key hash matches, or <see langword="null"/> if not found.</summary>
    /// <param name="keyHash">The hashed API key to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Guid?> FindActiveGuidByKeyHash(string keyHash, CancellationToken cancellationToken = default);
}
