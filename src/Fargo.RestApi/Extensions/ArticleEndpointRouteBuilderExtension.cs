using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Domain.ValueObjects;

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
                        IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/articles",
                    async (
                        DateTime? atDateTime,
                        int? page,
                        int? limit,
                        IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleAllQuery(atDateTime, new Pagination(page, limit)), cancellationToken));

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
                        ArticleUpdateDto dto,
                        ICommandHandlerAsync<ArticleUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleUpdateCommand(articleGuid, dto), cancellationToken));

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
