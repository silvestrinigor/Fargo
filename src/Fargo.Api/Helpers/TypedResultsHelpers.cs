using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Helpers;

/// <summary>
/// Helper methods for handling typed results in REST API responses.
/// </summary>
public static class TypedResultsHelpers
{
    /// <summary>
    /// Handles a single query result and returns an appropriate typed result.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of the response.
    /// </typeparam>
    /// <param name="response">
    /// The query result to handle.
    /// </param>
    /// <returns>
    /// <see cref="TypedResults.Ok{TValue}(TValue)"/> when the response exists;
    /// otherwise <see cref="TypedResults.NotFound()"/>.
    /// </returns>
    public static Results<Ok<TResponse>, NotFound> HandleQueryResult<TResponse>(TResponse? response)
    {
        if (response == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(response);
    }

    /// <summary>
    /// Handles a nullable read-only collection query result and returns an appropriate typed result.
    /// </summary>
    /// <typeparam name="TResponseItem">
    /// The type of items in the response collection.
    /// </typeparam>
    /// <param name="response">
    /// The query result to handle.
    /// </param>
    /// <returns>
    /// <see cref="TypedResults.NotFound()"/> when the response is <see langword="null"/>;
    /// <see cref="TypedResults.NoContent()"/> when the collection is empty;
    /// otherwise <see cref="TypedResults.Ok{TValue}(TValue)"/>.
    /// </returns>
    public static Results<Ok<IReadOnlyCollection<TResponseItem>>, NotFound, NoContent>
        HandleNullableCollectionQueryResult<TResponseItem>(
                IReadOnlyCollection<TResponseItem>? response)
    {
        if (response == null)
        {
            return TypedResults.NotFound();
        }

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    /// <summary>
    /// Handles a non-null read-only collection query result and returns an appropriate typed result.
    /// </summary>
    /// <typeparam name="TResponseItem">
    /// The type of items in the response collection.
    /// </typeparam>
    /// <param name="response">
    /// The non-null query result to handle.
    /// </param>
    /// <returns>
    /// <see cref="TypedResults.NoContent()"/> when the collection is empty;
    /// otherwise <see cref="TypedResults.Ok{TValue}(TValue)"/>.
    /// </returns>
    public static Results<Ok<IReadOnlyCollection<TResponseItem>>, NoContent>
        HandleCollectionQueryResult<TResponseItem>(
                IReadOnlyCollection<TResponseItem> response)
    {
        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }
}
