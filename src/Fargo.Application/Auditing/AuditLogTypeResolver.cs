using Fargo.Application.Commands.Articles;
using Fargo.Application.Commands.Identity;
using Fargo.Application.Commands.Initialization;
using Fargo.Application.Commands.Items;
using Fargo.Application.Commands.Partitions;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Commands.Users;
using Fargo.Core.Audits;

namespace Fargo.Application.Auditing;

/// <remarks>
/// The mapping is explicit and total: an unmapped command throws, so a new command cannot be
/// audited with a silently defaulted classification.
/// </remarks>
public static class AuditLogTypeResolver
{
    private static readonly IReadOnlyDictionary<Type, AuditLogType> map = new Dictionary<Type, AuditLogType>
    {
        // Articles
        [typeof(ArticleCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(ArticleUpdateCommand)] = AuditLogType.EntityUpdated,
        [typeof(ArticleSetPartitionsCommand)] = AuditLogType.EntityUpdated,
        [typeof(ArticleDeleteCommand)] = AuditLogType.EntityDeleted,

        // Items
        [typeof(ItemCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(ItemSetPartitionsCommand)] = AuditLogType.EntityUpdated,
        [typeof(ItemSetLocationCommand)] = AuditLogType.EntityUpdated,
        [typeof(ItemRemoveFromContainerCommand)] = AuditLogType.EntityUpdated,
        [typeof(ItemDeleteCommand)] = AuditLogType.EntityDeleted,
        [typeof(ItemEventCreateCommand)] = AuditLogType.ItemEventRecorded,

        // Partitions
        [typeof(PartitionCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(PartitionUpdateCommand)] = AuditLogType.EntityUpdated,
        [typeof(PartitionSetParentCommand)] = AuditLogType.EntityUpdated,
        [typeof(PartitionDeleteCommand)] = AuditLogType.EntityDeleted,

        // Users
        [typeof(UserCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(UserUpdateCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserSetActiveCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserSetPermissionsCommand)] = AuditLogType.PermissionsChanged,
        [typeof(UserSetUserGroupsCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserSetPartitionsCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserResetPasswordCommand)] = AuditLogType.PasswordChanged,
        [typeof(UserSetPasswordExpirationCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserRemovePasswordExpirationCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserDeleteCommand)] = AuditLogType.EntityDeleted,

        // User groups
        [typeof(UserGroupCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(UserGroupUpdateCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserGroupSetActiveCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserGroupSetPermissionsCommand)] = AuditLogType.PermissionsChanged,
        [typeof(UserGroupSetPartitionsCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserGroupSetParentCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserGroupRemoveParentCommand)] = AuditLogType.EntityUpdated,
        [typeof(UserGroupDeleteCommand)] = AuditLogType.EntityDeleted,

        // Service accounts
        [typeof(ServiceAccountCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(ServiceAccountUpdateCommand)] = AuditLogType.EntityUpdated,
        [typeof(ServiceAccountSetActiveCommand)] = AuditLogType.EntityUpdated,
        [typeof(ServiceAccountSetPermissionsCommand)] = AuditLogType.PermissionsChanged,
        [typeof(ServiceAccountSetPartitionsCommand)] = AuditLogType.EntityUpdated,
        [typeof(ServiceAccountDeleteCommand)] = AuditLogType.EntityDeleted,
        [typeof(ServiceAccountRotateSecretCommand)] = AuditLogType.ServiceAccountSecretRotated,

        // Identity
        [typeof(IdentityLoginCommand)] = AuditLogType.Authenticated,
        [typeof(IdentityLogoutCommand)] = AuditLogType.LoggedOut,
        [typeof(IdentityRefreshCommand)] = AuditLogType.TokenRefreshed,
        [typeof(IdentityPasswordChangeCommand)] = AuditLogType.PasswordChanged,
        [typeof(ServiceTokenCommand)] = AuditLogType.ServiceTokenIssued,

        // System
        [typeof(InitializeSystemCommand)] = AuditLogType.SystemInitialized,

        // Supply orders
        [typeof(SupplyOrderCreateCommand)] = AuditLogType.EntityCreated,
        [typeof(SupplyOrderAddLineCommand)] = AuditLogType.EntityUpdated,
        [typeof(SupplyOrderChangeLineQuantityCommand)] = AuditLogType.EntityUpdated,
        [typeof(SupplyOrderRemoveLineCommand)] = AuditLogType.EntityUpdated,
        [typeof(SupplyOrderSetReceivingContainerCommand)] = AuditLogType.EntityUpdated,
        [typeof(SupplyOrderPlaceCommand)] = AuditLogType.SupplyOrderPlaced,
        [typeof(SupplyOrderReceiveCommand)] = AuditLogType.SupplyOrderReceived,
        [typeof(SupplyOrderCancelCommand)] = AuditLogType.SupplyOrderCancelled,
        [typeof(SupplyOrderCloseCommand)] = AuditLogType.SupplyOrderClosed,
    };

    /// <exception cref="NotSupportedException">
    /// <paramref name="commandType"/> has no mapping.
    /// </exception>
    public static AuditLogType Resolve(Type commandType)
    {
        ArgumentNullException.ThrowIfNull(commandType);

        if (map.TryGetValue(commandType, out var auditLogType))
        {
            return auditLogType;
        }

        throw new NotSupportedException(
            $"No {nameof(AuditLogType)} is mapped for command type '{commandType.FullName}'. " +
            $"Add an entry to {nameof(AuditLogTypeResolver)}.");
    }
}
