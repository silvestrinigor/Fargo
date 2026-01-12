using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Commom
{
    public static class TypedResultsHelpers
    {
        public static Results<Ok<TResponse>, NotFound> HandleQueryResult<TResponse>(TResponse? response)
        {
            if (response == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        }

        public static Results<Ok<IEnumerable<TResponseItem>>, NotFound, NoContent> HandleQueryResult<TResponseItem>(IEnumerable<TResponseItem>? response)
        {
            if (response == null)
            {
                return TypedResults.NotFound();
            }

            if (response.Any())
            {
                return TypedResults.NoContent();
            }

            return TypedResults.Ok(response);
        }
    }
}
