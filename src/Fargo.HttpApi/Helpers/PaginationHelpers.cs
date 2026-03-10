using Fargo.Application.Common;

namespace Fargo.HttpApi.Helpers
{
    /// <summary>
    /// Provides helper methods for handling pagination parameters
    /// received from HTTP requests.
    /// </summary>
    public static class PaginationHelpers
    {
        /// <summary>
        /// Creates a valid <see cref="Pagination"/> instance using optional
        /// page and limit parameters.
        /// </summary>
        /// <param name="page">
        /// The page index provided by the client.
        /// If <c>null</c>, <see cref="Page.FirstPage"/> is used.
        /// </param>
        /// <param name="limit">
        /// The maximum number of items per page provided by the client.
        /// If <c>null</c>, <see cref="Limit.MaxLimit"/> is used.
        /// </param>
        /// <returns>
        /// A valid <see cref="Pagination"/> instance with default values applied
        /// when parameters are not provided.
        /// </returns>
        public static Pagination CreatePagination(Page? page, Limit? limit)
        {
            return new Pagination(
                page ?? Page.FirstPage,
                limit ?? Limit.MaxLimit
            );
        }
    }
}