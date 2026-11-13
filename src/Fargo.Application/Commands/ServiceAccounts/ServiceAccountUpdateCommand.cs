using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.ServiceAccounts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.ServiceAccounts;

public sealed record ServiceAccountUpdateCommand(
    Guid ServiceAccountGuid,
    ServiceAccountUpdateDto Update
) : ICommand;

public sealed class ServiceAccountUpdateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountUpdateCommand>
{
    public async Task HandleAsync(ServiceAccountUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(account.Actor);

        if (command.Update.Description is { } description)
        {
            account.Actor.Description = new Description(description);
        }
    }
}
