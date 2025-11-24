using Fargo.Application.Interfaces.Solicitations.Commands;
using Fargo.Application.Solicitations.Commands.ContainerCommands.CreateContainer;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ContainerEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoContainer()
            {
                builder.MapGet("/containers", async ([FromServices] IContainerApplicationService service)
                    => await service.GetContainerAsync());

                builder.MapGet("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService service)
                    => await service.GetContainerAsync(container));

                builder.MapGet("/containers/guids", async ([FromServices] IContainerApplicationService service)
                    => await service.GetContainersGuidsAsync());

                builder.MapGet("/containers/{container}/entities", async (Guid container, [FromServices] IContainerApplicationService service)
                    => await service.GetContainerEntitiesAsync(container));

                builder.MapPost("/containers", async ([FromBody] CreateContainerCommand articleCreateCommand, [FromServices] ICommandHandler<CreateContainerCommand, Task> handler)
                    => await handler.Handle(articleCreateCommand));

                builder.MapPatch("/containers/{container}", async (Guid container, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IContainerApplicationService service)
                    => await service.UpdateContainerAsync(container, containerUpdateDto));

                builder.MapPut("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService service)
                    => await service.InsertEntityIntoContainerAsync(container, entity));

                builder.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerApplicationService service)
                    => await service.DeleteContainerAsync(container));

                builder.MapDelete("/containers/{container}/entities/{entity}", async (Guid container, Guid entity, [FromServices] IContainerApplicationService service)
                    => await service.RemoveEntityFromContainerAsync(container, entity));
            }
        }
    }
}