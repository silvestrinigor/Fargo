using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Commom;
using Microsoft.AspNetCore.Http.HttpResults;

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
                        DateTime? asOfDateTime,
                        IQueryHandlerAsync<ArticleSingleQuery, ArticleReadModel?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleSingleQuery(articleGuid, asOfDateTime);

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "article/",
                    async (
                        DateTime? asOfDateTime,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<ArticleManyQuery, IEnumerable<ArticleReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleManyQuery(asOfDateTime, new Pagination(page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "article/",
                    async (
                        ArticleCreateCommand command,
                        ICommandHandlerAsync<ArticleCreateCommand, Guid> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapPatch(
                    "article/{articleGuid}",
                    async (
                        Guid articleGuid, 
                        ArticleUpdateModel model, 
                        ICommandHandlerAsync<ArticleUpdateCommand> handler, 
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new ArticleUpdateCommand(articleGuid, model), cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapDelete(
                    "article/{articleGuid}",
                    async (
                        Guid articleGuid,
                        ICommandHandlerAsync<ArticleDeleteCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new ArticleDeleteCommand(articleGuid), cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}