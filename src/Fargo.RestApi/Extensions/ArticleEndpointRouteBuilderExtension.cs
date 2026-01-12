using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;

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
                        DateTime? atDateTime,
                        IQueryHandlerAsync<ArticleSingleQuery, ArticleReadModel?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/articles",
                    async (
                        DateTime? atDateTime,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<ArticleManyQuery, IEnumerable<ArticleReadModel>> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(
                        new ArticleManyQuery(atDateTime, new Pagination(page ?? default, limit ?? default)), 
                        cancellationToken));

                builder.MapPost(
                    "/articles",
                    async (
                        ArticleCreateCommand command,
                        ICommandHandlerAsync<ArticleCreateCommand, Guid> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapPatch(
                    "/articles/{articleGuid}",
                    async (
                        Guid articleGuid,
                        ArticleUpdateModel model,
                        ICommandHandlerAsync<ArticleUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleUpdateCommand(articleGuid, model), cancellationToken));

                builder.MapDelete(
                    "/articles/{articleGuid}",
                    async (
                        Guid articleGuid,
                        ICommandHandlerAsync<ArticleDeleteCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleDeleteCommand(articleGuid), cancellationToken));
            }
        }
    }
}