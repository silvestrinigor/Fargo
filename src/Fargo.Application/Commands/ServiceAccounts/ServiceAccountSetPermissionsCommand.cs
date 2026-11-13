using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;

namespace Fargo.Application.Commands.ServiceAccounts;

public sealed record ServiceAccountSetPermissionsCommand(Guid ServiceAccountGuid, PermissionsChangeDto Change) : ICommand;

public sealed class ServiceAccountSetPermissionsCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountSetPermissionsCommand>
{
    public async Task HandleAsync(
        ServiceAccountSetPermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeServiceAccountPermissions);

        var (toAdd, toRemove) = ActorPermissionChangeExtensions.ValidatePermissionsChange(command.Change);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(account.Actor);

        actor.ThrowIfChangingOwnPermissions(account.ActorGuid, ActorType.Service);

        actor.ApplyPermissionsChange(toAdd, toRemove, account.Actor.AddPermission, account.Actor.RemovePermission);
    }
}
