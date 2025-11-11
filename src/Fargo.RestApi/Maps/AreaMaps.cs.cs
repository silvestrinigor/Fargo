using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Maps;

public static class AreaMaps
{
    public static void MapFargoArea(this IEndpointRouteBuilder webApplication)
    {
        webApplication.MapGet("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.GetAreaAsync(area));

        webApplication.MapGet("/areas/", async ([FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.GetAreaAsync());

        webApplication.MapGet("/areas/guids/", async ([FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.GetAreaGuidsAsync());

        webApplication.MapPost("/areas/", async ([FromBody] EntityCreateDto areaCreateDto, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.CreateAreaAsync(areaCreateDto));

        webApplication.MapPatch("/areas/{area}", async (Guid area, [FromBody] EntityUpdateDto areaUpdateDto, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.UpdateAreaAsync(area, areaUpdateDto));

        webApplication.MapDelete("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.DeleteAreaAsync(area));

        webApplication.MapGet("/areas/{area}/entities/", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.GetAreaEntitiesAsync(area));

        webApplication.MapPut("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.InsertEntityIntoAreaAsync(area, entity));

        webApplication.MapDelete("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
            => await areaApplicationService.RemoveEntityFromAreaAsync(area, entity));
    }
}
