using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Fargo.HttpApi.EndpointRouteBuilder;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class AreaEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArea()
            {
                builder.MapGet("/areas", async ([FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.GetAreaAsync());

                builder.MapGet("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.GetAreaAsync(area));

                builder.MapGet("/areas/guids", async ([FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.GetAreaGuidsAsync());

                builder.MapGet("/areas/{area}/entities", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.GetAreaEntitiesAsync(area));

                builder.MapPost("/areas", async ([FromBody] EntityCreateDto areaCreateDto, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.CreateAreaAsync(areaCreateDto));

                builder.MapPatch("/areas/{area}", async (Guid area, [FromBody] EntityUpdateDto areaUpdateDto, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.UpdateAreaAsync(area, areaUpdateDto));

                builder.MapPut("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.InsertEntityIntoAreaAsync(area, entity));

                builder.MapDelete("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.DeleteAreaAsync(area));
                
                builder.MapDelete("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService areaApplicationService)
                    => await areaApplicationService.RemoveEntityFromAreaAsync(area, entity));
            }
        }
    }
}