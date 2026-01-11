using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Queries.ItemQueries;

namespace Fargo.HttpApi.Extensions
{
    public static class ItemEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoItem()
            {
                builder.MapGet(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        DateTime? atDateTime,
                        IQueryHandlerAsync<ItemSingleQuery, ItemReadModel?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/items",
                    async (
                        Guid? parentItemGuid,
                        Guid? articleGuid,
                        DateTime? atDateTime,
                        int? page,
                        int? limit,
                        IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemReadModel>> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(
                        new ItemManyQuery(parentItemGuid, articleGuid, atDateTime, new Pagination(page, limit)), cancellationToken));

                builder.MapPost(
                    "/items",
                    async (
                        ItemCreateCommand command,
                        ICommandHandlerAsync<ItemCreateCommand, Guid> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapPatch(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ItemUpdateModel model,
                        ICommandHandlerAsync<ItemUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemUpdateCommand(itemGuid, model), cancellationToken));

                builder.MapDelete(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ICommandHandlerAsync<ItemDeleteCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemDeleteCommand(itemGuid), cancellationToken));
            }
        }
    }
}