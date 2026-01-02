using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.ItensCommands;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.HttpApi.EndpointRouteBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilders
{
    public static class ItemEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoItem()
            {
                builder.MapGet("/items/{itemGuid}", async (Guid itemGuid, [FromServices] IQueryHandlerAsync<ItemSingleQuery, ItemDto> handler)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid)));

                builder.MapPost("/items", async ([FromBody] ItemCreateCommand command, [FromServices] ICommandHandlerAsync<ItemCreateCommand, Guid> handler)
                    => await handler.HandleAsync(command));

                builder.MapDelete("/items/{itemGuid}", async (Guid itemGuid, [FromServices] ICommandHandlerAsync<ItemDeleteCommand> handler)
                    => await handler.HandleAsync(new ItemDeleteCommand(itemGuid)));
            }
        }
    }
}
