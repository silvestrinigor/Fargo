using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Helpers;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Provides extension methods for routing articles in the API.
    /// </summary>
    public static class ArticleEndpointRouteBuilderExtension
    {
        /// <summary>
        /// Adds routes related to articles to the endpoint route builder.
        /// </summary>
        /// <param name="builder">The endpoint route builder.</param>
        extension(IEndpointRouteBuilder builder)
        {
            /// <summary>
            /// Maps the article endpoints to the route builder.
            /// </summary>
            public void MapFargoArticle()
            {
                /// <summary>
                /// Gets an article by its unique identifier.
                /// </summary>
                /// <param name="articleGuid">The unique identifier of the article.</param>
                /// <param name="temporalAsOf">The optional temporal as-of date to fetch the article.</param>
                /// <param name="handler">The query handler for fetching an article.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                builder.MapGet(
                        "/articles/{articleGuid}",
                        async (
                            Guid articleGuid,
                            DateTime? temporalAsOf,
                            IQueryHandler<ArticleSingleQuery, Task<ArticleReadModel?>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                /// <summary>
                /// Gets a list of articles.
                /// </summary>
                /// <param name="temporalAsOf">The optional temporal as-of date to fetch the articles.</param>
                /// <param name="page">The optional page number for pagination.</param>
                /// <param name="limit">The optional limit for pagination.</param>
                /// <param name="handler">The query handler for fetching multiple articles.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                builder.MapGet(
                        "/articles",
                        async (
                            DateTime? temporalAsOf,
                            Page? page,
                            Limit? limit,
                            IQueryHandler<ArticleManyQuery, Task<IEnumerable<ArticleReadModel>>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new ArticleManyQuery(
                                temporalAsOf,
                                new(page ?? default, limit ?? default)
                                );

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                /// <summary>
                /// Creates a new article.
                /// </summary>
                /// <param name="command">The command to create an article.</param>
                /// <param name="handler">The command handler for creating an article.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                builder.MapPost(
                        "/articles",
                        async (
                            ArticleCreateCommand command,
                            ICommandHandler<ArticleCreateCommand, Task<Guid>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var response = await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok(response);
                        });

                /// <summary>
                /// Updates an existing article.
                /// </summary>
                /// <param name="articleGuid">The unique identifier of the article to update.</param>
                /// <param name="model">The model containing the updated data for the article.</param>
                /// <param name="handler">The command handler for updating an article.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                builder.MapPatch(
                        "/articles/{articleGuid}",
                        async (
                            Guid articleGuid,
                            ArticleUpdateModel model,
                            ICommandHandler<ArticleUpdateCommand, Task> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new ArticleUpdateCommand(articleGuid, model);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });

                /// <summary>
                /// Deletes an existing article.
                /// </summary>
                /// <param name="articleGuid">The unique identifier of the article to delete.</param>
                /// <param name="handler">The command handler for deleting an article.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                builder.MapDelete(
                        "/articles/{articleGuid}",
                        async (
                            Guid articleGuid,
                            ICommandHandler<ArticleDeleteCommand, Task> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new ArticleDeleteCommand(articleGuid);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });
            }
        }
    }
}