using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;

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

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.ServiceAccount);

        var secret = secretGenerator.Generate();

        account.RotateSecret(passwordHasher.Hash(secret));

        var audit = AuditLog.CreateAuditLogForGlobalEntity(actor, account, AuditLogType.EntityUpdated);

        auditLogRepository.Add(audit);

        return new ServiceAccountCreatedDto(account.Guid, account.ClientId.ToString(), secret.Value);
    }
}
