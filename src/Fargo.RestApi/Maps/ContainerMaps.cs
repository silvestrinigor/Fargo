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

        webApplication.MapPost("/containers/", async ([FromBody] EntityDto articleCreateDto, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.CreateContainerAsync(articleCreateDto));

        webApplication.MapPatch("/containers/", async ([FromBody] EntityDto articleUpdateDto, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.UpdateContainerAsync(articleUpdateDto));

        webApplication.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.DeleteContainerAsync(container));

        webApplication.MapPut("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService containerApplicationService)
            => await containerApplicationService.InsertEntityIntoContainer(container, entity));
    }
}