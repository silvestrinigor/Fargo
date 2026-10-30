using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountCreateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
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
                account.AddPermission(permission);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { } partitionAccesses)
        {
            foreach (var partitionGuid in partitionAccesses)
            {
                account.AddPartitionAccess(partitionGuid);
            }
        }

        serviceAccountRepository.Add(account);

        var audit = AuditLog.CreateAuditLogForGlobalEntity(actor, account, AuditLogType.EntityCreated);

        auditLogRepository.Add(audit);

        return new ServiceAccountCreatedDto(account.Guid, account.ClientId.ToString(), secret.Value);
    }
}
