using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ContainerEndpointRouteBuilder
    {
        public static void MapFargoContainer(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.GetContainerAsync(container));

            endpointRouteBuilder.MapGet("/containers", async ([FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.GetContainerAsync());

            endpointRouteBuilder.MapGet("/containers/guids", async ([FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.GetContainersGuidsAsync());

            endpointRouteBuilder.MapPost("/containers", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.CreateContainerAsync(articleCreateDto));

            endpointRouteBuilder.MapPatch("/containers/{container}", async (Guid container, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.UpdateContainerAsync(container, containerUpdateDto));

            endpointRouteBuilder.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.DeleteContainerAsync(container));

            endpointRouteBuilder.MapGet("/containers/{container}/entities", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.GetContainerEntitiesAsync(container));

            endpointRouteBuilder.MapPut("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.InsertEntityIntoContainerAsync(container, entity));

            endpointRouteBuilder.MapDelete("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
                => await containerApplicationService.RemoveEntityFromContainerAsync(container, entity));
        }
    }
}