using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

public sealed record ApiClientUpdateCommand(Guid Guid, string? Name, string? Description, bool? IsActive) : ICommand;

public sealed class ApiClientUpdateCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientUpdateCommand>
{
    public async Task Handle(ApiClientUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        var client = await apiClientRepository.GetFoundByGuid(command.Guid, cancellationToken);

        if (command.Name is not null)
            client.Name = new(command.Name);

        if (command.Description is not null)
            client.Description = new(command.Description);

        if (command.IsActive.HasValue)
            client.IsActive = command.IsActive.Value;

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
