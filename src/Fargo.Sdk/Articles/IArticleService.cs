namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides CRUD operations for articles and routes hub Updated/Deleted events to tracked entities.
/// </summary>
public interface IArticleService
{
    /// <summary>Gets a single article by its unique identifier.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the article does not exist or is not accessible.</exception>
    Task<Article> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of articles accessible to the current user.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new article and returns it as a live entity.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Article> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an article. The article must have no associated items.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the article cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default);
}
