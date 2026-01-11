using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Domain.ValueObjects;

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
                        IQueryHandlerAsync<ItemSingleQuery, ItemDto?> handler,
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
                        IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>> handler,
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
                        ItemUpdateDto dto,
                        ICommandHandlerAsync<ItemUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemUpdateCommand(itemGuid, dto), cancellationToken));

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