using Fargo.Application.Services;
using Fargo.Application.Solicitations.Commands.ItemCommands;
using Fargo.Application.Solicitations.Queries.ItensQueries;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ItemEndpointRouteBuilder
    {
        extension (IEndpointRouteBuilder builder)
        {
            public void MapFargoItem()
            {
                builder.MapGet("/items/{item}", async (Guid item, [FromServices] IItemService service)
                    => await service.GetItemAsync(new GetItemQuery(item)));

                builder.MapPost("/items", async ([FromBody] CreateItemCommand command, [FromServices] IItemService service)
                    => await service.CreateItemAsync(command));

                builder.MapDelete("/items/{item}", async (Guid item, [FromServices] IItemService service)
                    => await service.DeleteItemAsync(new DeleteItemCommand(item)));
            }
        }
    }
}
