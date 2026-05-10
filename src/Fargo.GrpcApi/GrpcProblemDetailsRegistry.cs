using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.GrpcApi;

internal static class GrpcProblemDetailsRegistry
{
    private static readonly Dictionary<Type, GrpcProblemDetailsDefinition> Map = new()
    {
        { typeof(UnauthorizedAccessFargoApplicationException), new(401, "Unauthorized", "auth/unauthorized") },
        { typeof(InvalidCredentialsFargoApplicationException), new(401, "Invalid credentials", "auth/invalid-credentials") },
        { typeof(InvalidPasswordFargoApplicationException), new(400, "Invalid password", "auth/invalid-password") },
        { typeof(WeakPasswordFargoApplicationException), new(400, "Weak password", "auth/weak-password") },
        { typeof(InvalidNameidFargoApplicationException), new(400, "Invalid nameid", "user/invalid-nameid") },
        { typeof(PasswordChangeRequiredFargoApplicationException), new(403, "Password change required", "auth/password-change-required") },
        { typeof(ArticleNotFoundFargoApplicationException), new(404, "Article not found", "article/not-found") },
        { typeof(UserNotFoundFargoApplicationException), new(404, "User not found", "user/not-found") },
        { typeof(ItemNotFoundFargoApplicationException), new(404, "Item not found", "item/not-found") },
        { typeof(UserGroupNotFoundFargoApplicationException), new(404, "User group not found", "user-group/not-found") },
        { typeof(UserNotAuthorizedFargoDomainException), new(403, "Forbidden", "user/forbidden") },
        { typeof(ArticleDeleteWithItemsAssociatedFargoDomainException), new(400, "Invalid operation", "article/delete-with-items") },
        { typeof(ArticleBarcodeAlreadyInUseFargoDomainException), new(409, "Conflict", "barcode/already-exists") },
        { typeof(UserNameidAlreadyExistsDomainException), new(409, "Conflict", "user/nameid-already-exists") },
        { typeof(UserGroupNameidAlreadyExistsDomainException), new(409, "Conflict", "user-group/nameid-already-exists") },
        { typeof(UserCannotDeleteSelfFargoDomainException), new(400, "Invalid operation", "user/cannot-delete-self") },
        { typeof(UserCannotChangeOwnPermissionsFargoDomainException), new(400, "Invalid operation", "user/cannot-change-own-permissions") },
        { typeof(UserInactiveFargoDomainException), new(403, "User inactive", "user/inactive") },
        { typeof(PartitionNotFoundFargoApplicationException), new(404, "Partition not found", "partition/not-found") },
        { typeof(UserNotAuthorizedFargoApplicationException), new(403, "Forbidden", "user/forbidden") },
        { typeof(PartitionAccessDeniedFargoApplicationException), new(403, "Access denied", "partition/access-denied") },
        { typeof(PartitionedEntityAccessDeniedFargoApplicationException), new(403, "Access denied", "entity/access-denied") },
        { typeof(EntityAccessViolationFargoApplicationException), new(403, "Access denied", "entity/access-denied") },
        { typeof(UserGroupInactiveFargoDomainException), new(403, "User group inactive", "user-group/inactive") },
        { typeof(PartitionCircularHierarchyFargoDomainException), new(400, "Invalid operation", "partition/circular-hierarchy") },
        { typeof(PartitionCannotBeOwnParentFargoDomainException), new(400, "Invalid operation", "partition/cannot-be-own-parent") },
        { typeof(ItemCannotBeOwnContainerFargoDomainException), new(400, "Invalid operation", "item/cannot-be-own-container") },
        { typeof(ItemParentIsNotContainerFargoDomainException), new(400, "Invalid operation", "item/parent-is-not-container") },
        { typeof(ItemCircularContainerHierarchyFargoDomainException), new(400, "Invalid operation", "item/circular-container-hierarchy") },
        { typeof(DeleteMainAdminUserFargoDomainException), new(400, "Invalid operation", "user/cannot-delete-main-admin") },
        { typeof(DeleteDefaultAdministratorsUserGroupFargoDomainException), new(400, "Invalid operation", "user-group/cannot-delete-default-admins") },
        { typeof(ChangeMainAdminUserPermissionsFargoDomainException), new(400, "Invalid operation", "user/cannot-change-main-admin-permissions") },
        { typeof(PartitionGlobalDeleteFargoDomainException), new(400, "Invalid operation", "partition/cannot-delete-global") },
        { typeof(UserCannotDeleteParentUserGroupFargoDomainException), new(400, "Invalid operation", "user-group/cannot-delete-parent-group") },
        { typeof(ArgumentException), new(400, "Invalid request", "request/invalid") },
        { typeof(ArgumentNullException), new(400, "Invalid request", "request/invalid") },
        { typeof(ArgumentOutOfRangeException), new(400, "Invalid request", "request/invalid") }
    };

    public static bool TryGetDefinition(Type exceptionType, out GrpcProblemDetailsDefinition definition)
        => Map.TryGetValue(exceptionType, out definition!);
}
