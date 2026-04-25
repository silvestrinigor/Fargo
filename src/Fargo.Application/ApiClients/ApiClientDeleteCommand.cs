using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

public sealed record ApiClientDeleteCommand(Guid Guid) : ICommand;

public sealed class ApiClientDeleteCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientDeleteCommand>
{
    public async Task Handle(ApiClientDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteApiClient);

        var client = await apiClientRepository.GetFoundByGuid(command.Guid, cancellationToken);

        apiClientRepository.Remove(client);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
