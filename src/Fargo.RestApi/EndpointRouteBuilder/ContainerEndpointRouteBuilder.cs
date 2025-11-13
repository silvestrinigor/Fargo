using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ContainerEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoContainer()
            {
                builder.MapGet("/containers", async ([FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.GetContainerAsync());

                builder.MapGet("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.GetContainerAsync(container));

                builder.MapGet("/containers/guids", async ([FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.GetContainersGuidsAsync());

                builder.MapGet("/containers/{container}/entities", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.GetContainerEntitiesAsync(container));

                builder.MapPost("/containers", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.CreateContainerAsync(articleCreateDto));

                builder.MapPatch("/containers/{container}", async (Guid container, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.UpdateContainerAsync(container, containerUpdateDto));

                builder.MapPut("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.InsertEntityIntoContainerAsync(container, entity));

                builder.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.DeleteContainerAsync(container));

                builder.MapDelete("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
                    => await containerApplicationService.RemoveEntityFromContainerAsync(container, entity));
            }
        }
    }
}