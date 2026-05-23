using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Application.Workspaces;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fargo.GrpcApi.Interceptors;

public sealed class FargoGrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw ToRpcException(exception);
        }
    }

    private static RpcException ToRpcException(Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var errorCode = GetErrorCode(exception);
        var message = exception.Message;

        var metadata = new Metadata
        {
            { "fargo-error-code", errorCode }
        };

        return new RpcException(new Status(statusCode, message), metadata);
    }

    private static StatusCode GetStatusCode(Exception exception)
        => exception switch
        {
            UnauthorizedAccessFargoApplicationException => StatusCode.Unauthenticated,
            InvalidCredentialsFargoApplicationException => StatusCode.Unauthenticated,
            PasswordChangeRequiredFargoApplicationException => StatusCode.PermissionDenied,
            UserNotAuthorizedFargoApplicationException => StatusCode.PermissionDenied,
            UserNotAuthorizedFargoDomainException => StatusCode.PermissionDenied,
            UserEntityAccessNotAuthorizedFargoDomainException => StatusCode.PermissionDenied,
            UserPartitionAccessNotAuthorizedFargoDomainException => StatusCode.PermissionDenied,
            PartitionAccessDeniedFargoApplicationException => StatusCode.PermissionDenied,
            PartitionedEntityAccessDeniedFargoApplicationException => StatusCode.PermissionDenied,
            EntityAccessViolationFargoApplicationException => StatusCode.PermissionDenied,
            ArticleNotFoundFargoApplicationException => StatusCode.NotFound,
            ItemNotFoundFargoApplicationException => StatusCode.NotFound,
            PartitionNotFoundFargoApplicationException => StatusCode.NotFound,
            WorkspaceNotFoundFargoApplicationException => StatusCode.NotFound,
            UserNotFoundFargoApplicationException => StatusCode.NotFound,
            UserGroupNotFoundFargoApplicationException => StatusCode.NotFound,
            ArticleBarcodeAlreadyInUseFargoDomainException => StatusCode.AlreadyExists,
            UserNameidAlreadyExistsDomainException => StatusCode.AlreadyExists,
            UserGroupNameidAlreadyExistsDomainException => StatusCode.AlreadyExists,
            ArgumentException => StatusCode.InvalidArgument,
            UnreservedGuidFargoApplicationException => StatusCode.InvalidArgument,
            InvalidPasswordFargoApplicationException => StatusCode.InvalidArgument,
            InvalidNameidFargoApplicationException => StatusCode.InvalidArgument,
            WeakPasswordFargoApplicationException => StatusCode.InvalidArgument,
            FargoDomainException => StatusCode.FailedPrecondition,
            FargoApplicationException => StatusCode.FailedPrecondition,
            _ => StatusCode.Internal
        };

    private static string GetErrorCode(Exception exception)
        => exception switch
        {
            UnauthorizedAccessFargoApplicationException => "auth/unauthorized",
            InvalidCredentialsFargoApplicationException => "auth/invalid-credentials",
            InvalidPasswordFargoApplicationException => "auth/invalid-password",
            WeakPasswordFargoApplicationException => "auth/weak-password",
            InvalidNameidFargoApplicationException => "user/invalid-nameid",
            PasswordChangeRequiredFargoApplicationException => "auth/password-change-required",
            ArticleNotFoundFargoApplicationException => "article/not-found",
            UserNotFoundFargoApplicationException => "user/not-found",
            ItemNotFoundFargoApplicationException => "item/not-found",
            UserGroupNotFoundFargoApplicationException => "user-group/not-found",
            UserNotAuthorizedFargoDomainException => "user/forbidden",
            UserNotAuthorizedFargoApplicationException => "user/forbidden",
            UserEntityAccessNotAuthorizedFargoDomainException => "entity/access-denied",
            UserPartitionAccessNotAuthorizedFargoDomainException => "partition/access-denied",
            ArticleDeleteWithItemsAssociatedFargoDomainException => "article/delete-with-items",
            ArticleBarcodeAlreadyInUseFargoDomainException => "barcode/already-exists",
            ArticleIsNotContainerFargoDomainException => "article/not-container",
            EntityNotActiveFargoDomainException<Article> => "article/inactive",
            UserNameidAlreadyExistsDomainException => "user/nameid-already-exists",
            UserGroupNameidAlreadyExistsDomainException => "user-group/nameid-already-exists",
            UserCannotDeleteSelfFargoDomainException => "user/cannot-delete-self",
            UserCannotChangeOwnPermissionsFargoDomainException => "user/cannot-change-own-permissions",
            UserInactiveFargoDomainException => "user/inactive",
            PartitionNotFoundFargoApplicationException => "partition/not-found",
            WorkspaceNotFoundFargoApplicationException => "workspace/not-found",
            UnreservedGuidFargoApplicationException => "workspace/unreserved-guid",
            PartitionAccessDeniedFargoApplicationException => "partition/access-denied",
            PartitionedEntityAccessDeniedFargoApplicationException => "entity/access-denied",
            EntityAccessViolationFargoApplicationException => "entity/access-denied",
            PartitionCircularHierarchyFargoDomainException => "partition/circular-hierarchy",
            PartitionCannotBeOwnParentFargoDomainException => "partition/cannot-be-own-parent",
            ItemCannotBeOwnContainerFargoDomainException => "item/cannot-be-own-container",
            ItemParentIsNotContainerFargoDomainException => "item/parent-is-not-container",
            ItemCircularContainerHierarchyFargoDomainException => "item/circular-container-hierarchy",
            DeleteMainAdminUserFargoDomainException => "user/cannot-delete-main-admin",
            DeleteDefaultAdministratorsUserGroupFargoDomainException => "user-group/cannot-delete-default-admins",
            ChangeMainAdminUserPermissionsFargoDomainException => "user/cannot-change-main-admin-permissions",
            PartitionGlobalDeleteFargoDomainException => "partition/cannot-delete-global",
            UserCannotDeleteParentUserGroupFargoDomainException => "user-group/cannot-delete-parent-group",
            ArgumentException => "request/invalid",
            _ when IsGenericEntityNotActive(exception) => "entity/inactive",
            _ => "server/internal-error"
        };

    private static bool IsGenericEntityNotActive(Exception exception)
    {
        var exceptionType = exception.GetType();

        return exceptionType.IsGenericType &&
            exceptionType.GetGenericTypeDefinition() == typeof(EntityNotActiveFargoDomainException<>);
    }
}
