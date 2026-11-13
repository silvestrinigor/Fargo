using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.ServiceAccounts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Commands.ServiceAccounts;

/// <summary>Returns the new one-time secret.</summary>
public sealed record ServiceAccountRotateSecretCommand(
    Guid ServiceAccountGuid
) : ICommand<ServiceAccountCreatedDto>;

public sealed class ServiceAccountRotateSecretCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IClientSecretGenerator secretGenerator,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto>
{
    public async Task<ServiceAccountCreatedDto> HandleAsync(ServiceAccountRotateSecretCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.RotateServiceAccountSecret);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(account.Actor);

        var secret = secretGenerator.Generate();

        account.RotateSecret(passwordHasher.Hash(secret));

        return new ServiceAccountCreatedDto(account.ActorGuid, account.Actor.Nameid.ToString(), secret.Value);
    }
}
