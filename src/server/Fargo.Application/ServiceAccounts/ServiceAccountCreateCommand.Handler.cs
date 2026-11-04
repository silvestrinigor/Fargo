using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountCreateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    IClientSecretGenerator secretGenerator,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto>
{
    public async Task<ServiceAccountCreatedDto> HandleAsync(ServiceAccountCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateServiceAccount);

        var clientId = new Nameid(command.Create.ClientId);

        var existing = await serviceAccountRepository.GetByClientIdAsync(clientId, cancellationToken);

        if (existing is not null)
        {
            throw new ServiceAccountClientIdUnavailableFargoApplicationException(command.Create.ClientId);
        }

        var secret = secretGenerator.Generate();

        var secretHash = passwordHasher.Hash(secret);

        var account = Actor.NewServiceAccount(clientId, secretHash).ServiceAccount!;
        var accountActor = account.Actor;

        if (command.Create.Description is { } description)
        {
            accountActor.Description = new Description(description);
        }

        if (command.Create.IsActive is false)
        {
            accountActor.Deactivate();
            account.RotateAuthVersion();
        }

        if (command.Create.PermissionsToAdd is { } permissions)
        {
            foreach (var permission in permissions)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.AddPermission(permission);
            }
        }

        if (command.Create.PartitionsToAdd is { } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartition(partition);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { } partitionAccesses)
        {
            foreach (var partitionGuid in partitionAccesses)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartitionAccess(partition);
            }
        }

        serviceAccountRepository.Add(account);

        // The audit log inherits the account's partition membership so it is
        // only readable by actors with access to those partitions. When the
        // account has no membership it has no partition scope, so the audit is
        // recorded against the global partition (as before) to avoid an
        // unpartitioned, globally-readable audit entry.
        var audit = accountActor.PartitionGuids.Count > 0
            ? AuditLog.CreateAuditLog(actor, accountActor, AuditLogType.EntityCreated)
            : AuditLog.CreateAuditLogForGlobalEntity(actor, accountActor, AuditLogType.EntityCreated);

        audit.Metadata.AddPermissions([.. accountActor.Permissions]);
        audit.Metadata.AddPartitions(accountActor.PartitionGuids);
        audit.Metadata.AddPartitionAccessesAdded([.. accountActor.PartitionAccesses.Select(a => a.PartitionGuid)]);

        auditLogRepository.Add(audit);

        return new ServiceAccountCreatedDto(accountActor.Guid, accountActor.Nameid.ToString(), secret.Value);
    }
}
