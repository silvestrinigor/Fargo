using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Helpers
{
    /// <summary>
    /// Helper methods for handling typed results in REST API responses.
    /// </summary>
    public static class TypedResultsHelpers
    {
        /// <summary>
        /// Handles a query result and returns an appropriate TypedResult based on the response.
        /// If the response is null, it returns NotFound. Otherwise, it returns Ok with the response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="response">The query result to handle.</param>
        /// <returns>A TypedResult containing either Ok or NotFound.</returns>
        public static Results<Ok<TResponse>, NotFound> HandleQueryResult<TResponse>(TResponse? response)
        {
            if (response == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        }

        /// <summary>
        /// Handles a query result and returns an appropriate TypedResult based on the response.
        /// If the response is null, it returns NotFound. If the response is empty, it returns NoContent. Otherwise, it returns Ok with the response.
        /// </summary>
        /// <typeparam name="TResponseItem">The type of items in the response.</typeparam>
        /// <param name="response">The query result to handle.</param>
        /// <returns>A TypedResult containing either Ok, NotFound, or NoContent.</returns>
        public static Results<Ok<IEnumerable<TResponseItem>>, NotFound, NoContent> HandleQueryResult<TResponseItem>(IEnumerable<TResponseItem>? response)
        {
            if (response == null)
            {
                return TypedResults.NotFound();
            }

            if (!response.Any())
            {
                return TypedResults.NoContent();
            }

            return TypedResults.Ok(response);
        }
    }
}