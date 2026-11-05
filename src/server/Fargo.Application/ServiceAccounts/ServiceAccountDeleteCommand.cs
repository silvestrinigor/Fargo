using Fargo.Application.Common;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to delete a service account.</summary>
public sealed record ServiceAccountDeleteCommand(
    Guid ServiceAccountGuid
) : ICommand;

public sealed class ServiceAccountDeleteCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountDeleteCommand>
{
    public async Task HandleAsync(ServiceAccountDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        serviceAccountRepository.Remove(account);
    }
}
