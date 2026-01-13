using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Commom;

namespace Fargo.HttpApi.Extensions
{
    public static class ArticleEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet(
                    "/articles/{articleGuid}",
                    async (
                        Guid articleGuid,
                        DateTime? temporalAsOf,
                        IQueryHandlerAsync<ArticleSingleQuery, ArticleReadModel?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "articles/",
                    async (
                        DateTime? temporalAsOf,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<ArticleManyQuery, IEnumerable<ArticleReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleManyQuery(temporalAsOf, new (page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "articles/",
                    async (
                        ArticleCreateCommand command,
                        ICommandHandlerAsync<ArticleCreateCommand, Guid> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapPatch(
                    "articles/{articleGuid}",
                    async (
                        Guid articleGuid, 
                        ArticleUpdateModel model, 
                        ICommandHandlerAsync<ArticleUpdateCommand> handler, 
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ArticleUpdateCommand(articleGuid, model);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapDelete(
                    "articles/{articleGuid}",
                    async (
                        Guid articleGuid,
                        ICommandHandlerAsync<ArticleDeleteCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ArticleDeleteCommand(articleGuid);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}