using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;

namespace Fargo.Application.Commands.ServiceAccounts;

public sealed record ServiceAccountSetActiveCommand(Guid ServiceAccountGuid, bool IsActive) : ICommand;

public sealed class ServiceAccountSetActiveCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountSetActiveCommand>
{
    public async Task HandleAsync(
        ServiceAccountSetActiveCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ActivateServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(account.Actor);

        if (command.IsActive)
        {
            account.Actor.Activate();
        }
        else
        {
            account.Actor.Deactivate();

            // Invalidates the tokens already issued to the account.
            account.RotateAuthVersion();
        }
    }
}
