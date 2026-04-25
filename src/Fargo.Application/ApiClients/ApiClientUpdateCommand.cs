using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

/// <summary>Command to update the properties of an existing API client.</summary>
/// <param name="Guid">The unique identifier of the API client to update.</param>
/// <param name="Name">The new name, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Description">The new description, or <see langword="null"/> to leave unchanged.</param>
/// <param name="IsActive">The new active state, or <see langword="null"/> to leave unchanged.</param>
public sealed record ApiClientUpdateCommand(Guid Guid, string? Name, string? Description, bool? IsActive) : ICommand;

/// <summary>Handles <see cref="ApiClientUpdateCommand"/>.</summary>
public sealed class ApiClientUpdateCommandHandler(
    ActorService actorService,
    IApiClientRepository apiClientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ApiClientUpdateCommand>
{
    /// <summary>Validates the actor's permission and applies the requested property changes.</summary>
    /// <param name="command">The command with the properties to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task Handle(ApiClientUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        var client = await apiClientRepository.GetFoundByGuid(command.Guid, cancellationToken);

        if (command.Name is not null)
        {
            client.Name = new(command.Name);
        }

        if (command.Description is not null)
        {
            client.Description = new(command.Description);
        }

        if (command.IsActive.HasValue)
        {
            client.IsActive = command.IsActive.Value;
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
