using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Maps;

public static class ContainerMaps
{
    public static void MapFargoContainer(this IEndpointRouteBuilder webApplication)
    {
        webApplication.MapGet("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.GetContainerAsync(container));

        webApplication.MapGet("/containers/", async ([FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.GetContainerAsync());

        webApplication.MapGet("/containers/guids/", async ([FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.GetContainersGuidsAsync());

        webApplication.MapPost("/containers/", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.CreateContainerAsync(articleCreateDto));

        webApplication.MapPatch("/containers/{container}", async (Guid container, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.UpdateContainerAsync(container, containerUpdateDto));

        webApplication.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.DeleteContainerAsync(container));

        webApplication.MapGet("/containers/{container}/entities/", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.GetContainerEntitiesAsync(container));

        webApplication.MapPut("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.InsertEntityIntoContainerAsync(container, entity));

        webApplication.MapDelete("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.RemoveEntityFromContainerAsync(container, entity));
    }
}