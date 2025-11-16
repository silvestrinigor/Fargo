using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class AreaEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArea()
            {
                builder.MapGet("/areas", async ([FromServices] IAreaApplicationService service)
                    => await service.GetAreaAsync());

                builder.MapGet("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService service)
                    => await service.GetAreaAsync(area));

                builder.MapGet("/areas/guids", async ([FromServices] IAreaApplicationService service)
                    => await service.GetAreaGuidsAsync());

                builder.MapGet("/areas/{area}/entities", async (Guid area, [FromServices] IAreaApplicationService service)
                    => await service.GetAreaEntitiesAsync(area));

                builder.MapPost("/areas", async ([FromBody] EntityCreateDto areaCreateDto, [FromServices] IAreaApplicationService service)
                    => await service.CreateAreaAsync(areaCreateDto));

                builder.MapPatch("/areas/{area}", async (Guid area, [FromBody] EntityUpdateDto areaUpdateDto, [FromServices] IAreaApplicationService service)
                    => await service.UpdateAreaAsync(area, areaUpdateDto));

                builder.MapPut("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService service)
                    => await service.AddEntityIntoAreaAsync(area, entity));

                builder.MapDelete("/areas/{area}", async (Guid area, [FromServices] IAreaApplicationService service)
                    => await service.DeleteAreaAsync(area));
                
                builder.MapDelete("/areas/{area}/entities/{entity}", async (Guid area, Guid entity, [FromServices] IAreaApplicationService service)
                    => await service.RemoveEntityFromAreaAsync(area, entity));
            }
        }
    }
}