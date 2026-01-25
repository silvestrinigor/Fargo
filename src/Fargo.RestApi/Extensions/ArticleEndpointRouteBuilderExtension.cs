using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.HttpApi.Helpers;

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
                        IQueryHandler<ArticleSingleQuery, Task<ArticleReadModel?>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleSingleQuery(articleGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "/articles",
                    async (
                        DateTime? temporalAsOf,
                        Page? page,
                        Limit? limit,
                        IQueryHandler<ArticleManyQuery, Task<IEnumerable<ArticleReadModel>>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ArticleManyQuery(temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

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