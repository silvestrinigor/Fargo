using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class AreaEndpointRouteBuilder
    {
        public static void MapFargoArea(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.GetAreaAsync(area));

            endpointRouteBuilder.MapGet("/areas", async ([FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.GetAreaAsync());

            endpointRouteBuilder.MapGet("/areas/guids", async ([FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.GetAreaGuidsAsync());

            endpointRouteBuilder.MapPost("/areas", async ([FromBody] EntityCreateDto areaCreateDto, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.CreateAreaAsync(areaCreateDto));

            endpointRouteBuilder.MapPatch("/areas/{area}", async (Guid area, [FromBody] EntityUpdateDto areaUpdateDto, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.UpdateAreaAsync(area, areaUpdateDto));

            endpointRouteBuilder.MapDelete("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.DeleteAreaAsync(area));

            endpointRouteBuilder.MapGet("/areas/{area}/entities", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.GetAreaEntitiesAsync(area));

            endpointRouteBuilder.MapPut("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.InsertEntityIntoAreaAsync(area, entity));

            endpointRouteBuilder.MapDelete("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
                => await areaApplicationService.RemoveEntityFromAreaAsync(area, entity));
        }
    }
}