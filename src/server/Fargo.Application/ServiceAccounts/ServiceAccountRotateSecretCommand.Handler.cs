using Fargo.Application.Common;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Security;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountRotateSecretCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IAuditLogRepository auditLogRepository,
    IClientSecretGenerator secretGenerator,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto>
{
    public async Task<ServiceAccountCreatedDto> HandleAsync(ServiceAccountRotateSecretCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        var secret = secretGenerator.Generate();

        account.RotateSecret(passwordHasher.Hash(secret));

        var audit = AuditLog.CreateAuditLogForGlobalEntity(actor, account.Actor, AuditLogType.EntityUpdated);

        auditLogRepository.Add(audit);

        return new ServiceAccountCreatedDto(account.ActorGuid, account.Actor.Nameid.ToString(), secret.Value);
    }
}
