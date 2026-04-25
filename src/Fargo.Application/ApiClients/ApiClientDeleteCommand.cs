using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

/// <summary>Command to delete an existing API client.</summary>
/// <param name="Guid">The unique identifier of the API client to delete.</param>
public sealed record ApiClientDeleteCommand(Guid Guid) : ICommand;

/// <summary>Handles <see cref="ApiClientDeleteCommand"/>.</summary>
public sealed class ApiClientDeleteCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientDeleteCommand>
{
    /// <summary>Validates the actor's permission and removes the API client.</summary>
    /// <param name="command">The command containing the API client identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task Handle(ApiClientDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteApiClient);

        var client = await apiClientRepository.GetFoundByGuid(command.Guid, cancellationToken);

        apiClientRepository.Remove(client);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
