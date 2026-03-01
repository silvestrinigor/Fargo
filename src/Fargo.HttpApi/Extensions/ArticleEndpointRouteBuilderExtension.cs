using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    public static class ArticleEndpointRouteBuilderExtension
    {
        public static void MapFargoArticle(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/articles")
                .RequireAuthorization();

            group.MapGet("/{articleGuid}", GetSingleArticle);
            group.MapGet("/", GetManyArticle);
            group.MapPost("/", CreateArticle);
            group.MapPatch("/{articleGuid}", UpdateArticle);
            group.MapDelete("/{articleGuid}", DeleteArticle);
        }

        private static async Task<Results<Ok<ArticleReadModel>, NotFound>> GetSingleArticle(
            Guid articleGuid,
            DateTime? temporalAsOf,
            IQueryHandler<ArticleSingleQuery, ArticleReadModel?> handler,
            CancellationToken cancellationToken)
        {
            var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IEnumerable<ArticleReadModel>>, NotFound, NoContent>> GetManyArticle(
            DateTime? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<ArticleManyQuery, IEnumerable<ArticleReadModel>> handler,
            CancellationToken cancellationToken)
        {
            var query = new ArticleManyQuery(
                temporalAsOf,
                new(page ?? default, limit ?? default)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
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