using Fargo.Application.Common;
using Fargo.Application.Models;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Extension responsible for mapping all Article endpoints.
    /// </summary>
    public static class ArticleEndpointRouteBuilderExtension
    {
        /// <summary>
        /// Maps all routes related to articles.
        /// </summary>
        /// <param name="builder">The endpoint route builder.</param>
        public static void MapFargoArticle(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/articles")
                .RequireAuthorization()
                .WithTags("Articles");

            group.MapGet("/{articleGuid:guid}", GetSingleArticle)
                .WithName("GetArticle")
                .WithSummary("Gets a single article")
                .WithDescription("Retrieves a single article by its unique identifier. Optionally allows querying historical data using temporal tables.")
                .Produces<ArticleReadModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", GetManyArticle)
                .WithName("GetArticles")
                .WithSummary("Gets multiple articles")
                .WithDescription("Retrieves a paginated list of articles. Supports optional temporal queries.")
                .Produces<IReadOnlyCollection<ArticleReadModel>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent);

            group.MapPost("/", CreateArticle)
                .WithName("CreateArticle")
                .WithSummary("Creates a new article")
                .WithDescription("Creates a new article and returns the generated identifier.")
                .Produces<Guid>(StatusCodes.Status200OK);

            group.MapPatch("/{articleGuid:guid}", UpdateArticle)
                .WithName("UpdateArticle")
                .WithSummary("Updates an existing article")
                .WithDescription("Updates an article using partial data.")
                .Produces(StatusCodes.Status204NoContent);

            group.MapDelete("/{articleGuid:guid}", DeleteArticle)
                .WithName("DeleteArticle")
                .WithSummary("Deletes an article")
                .WithDescription("Deletes the specified article from the system.")
                .Produces(StatusCodes.Status204NoContent);
        }

        private static async Task<Results<Ok<ArticleReadModel>, NotFound>> GetSingleArticle(
            Guid articleGuid,
            DateTimeOffset? temporalAsOf,
            IQueryHandler<ArticleSingleQuery, ArticleReadModel?> handler,
            CancellationToken cancellationToken)
        {
            var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IReadOnlyCollection<ArticleReadModel>>, NoContent>> GetManyArticle(
            DateTimeOffset? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleReadModel>> handler,
            CancellationToken cancellationToken)
        {
            var query = new ArticleManyQuery(
                temporalAsOf,
                PaginationHelpers.CreatePagination(page, limit)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleCollectionQueryResult(response);
        }

        private static async Task<Ok<Guid>> CreateArticle(
            ArticleCreateCommand command,
            ICommandHandler<ArticleCreateCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            var response = await handler.Handle(command, cancellationToken);

            return TypedResults.Ok(response);
        }

        private static async Task<NoContent> UpdateArticle(
            Guid articleGuid,
            ArticleUpdateModel model,
            ICommandHandler<ArticleUpdateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new ArticleUpdateCommand(articleGuid, model);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }

        private static async Task<NoContent> DeleteArticle(
            Guid articleGuid,
            ICommandHandler<ArticleDeleteCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new ArticleDeleteCommand(articleGuid);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}