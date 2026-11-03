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

        var account = ServiceAccount.Create(clientId, secretHash);

        if (command.Create.Description is { } description)
        {
            account.Description = new Description(description);
        }

        if (command.Create.IsActive is false)
        {
            account.Deactivate();
        }

        if (command.Create.PermissionsToAdd is { } permissions)
        {
            foreach (var permission in permissions)
            {
                actor.ThrowIfPermissionDenied(permission);

                account.AddPermission(permission);
            }
        }

        if (command.Create.PartitionsToAdd is { } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                account.AddPartition(partition);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { } partitionAccesses)
        {
            foreach (var partitionGuid in partitionAccesses)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                account.AddPartitionAccess(partition.Guid);
            }
        }

        serviceAccountRepository.Add(account);

        // The audit log inherits the account's partition membership so it is
        // only readable by actors with access to those partitions. When the
        // account has no membership it has no partition scope, so the audit is
        // recorded against the global partition (as before) to avoid an
        // unpartitioned, globally-readable audit entry.
        var audit = account.PartitionGuids.Count > 0
            ? AuditLog.CreateAuditLog(actor, account, AuditLogType.EntityCreated)
            : AuditLog.CreateAuditLogForGlobalEntity(actor, account, AuditLogType.EntityCreated);

        audit.Metadata.AddPermissions([.. account.Permissions]);
        audit.Metadata.AddPartitions(account.PartitionGuids);
        audit.Metadata.AddPartitionAccessesAdded([.. account.PartitionAccessGuids]);

        auditLogRepository.Add(audit);

        return new ServiceAccountCreatedDto(account.Guid, account.ClientId.ToString(), secret.Value);
    }
}
