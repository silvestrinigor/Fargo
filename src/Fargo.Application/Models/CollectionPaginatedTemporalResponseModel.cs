using Fargo.Application.Commom;

namespace Fargo.Application.Models
{
    /// <summary>
    /// Represents a paginated response containing a collection of entities.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of entity returned in the response.
    /// </typeparam>
    /// <param name="Entities">
    /// The collection of entities returned for the current page.
    /// </param>
    /// <param name="Page">
    /// The current page number of the paginated result.
    /// </param>
    /// <param name="Limit">
    /// The maximum number of entities returned per page.
    /// </param>
    public sealed record CollectionPaginatedTemporalResponseModel<TEntity>(
            IReadOnlyCollection<TEntity>? Entities,
            Page Page,
            Limit Limit
            );
}