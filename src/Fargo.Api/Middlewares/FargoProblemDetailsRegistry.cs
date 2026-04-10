using Fargo.Application.Exceptions;
using Fargo.Domain.Exceptions;

namespace Fargo.Api.Middlewares;

/// <summary>
/// Central registry that maps application and domain exceptions
/// to <see cref="ProblemDetailsDefinition"/> instances.
/// </summary>
/// <remarks>
/// This registry is used by the HTTP API exception middleware to convert
/// known exceptions into standardized HTTP responses following the
/// <c>application/problem+json</c> format.
///
/// Each mapped exception type defines:
/// <list type="bullet">
/// <item>
/// <description>The HTTP status code returned to the client.</description>
/// </item>
/// <item>
/// <description>A human-readable error title.</description>
/// </item>
/// <item>
/// <description>A machine-readable error type identifier.</description>
/// </item>
/// </list>
///
/// The goal is to ensure consistent error responses across the entire API
/// while keeping the mapping logic centralized and easy to maintain.
///
/// If an exception type is not present in this registry, the middleware
/// typically falls back to returning a generic <c>500 Internal Server Error</c>.
/// </remarks>
public static class FargoProblemDetailsRegistry
{
    /// <summary>
    /// Internal mapping between exception types and their corresponding
    /// <see cref="ProblemDetailsDefinition"/>.
    /// </summary>
    private static readonly Dictionary<Type, ProblemDetailsDefinition> map =
        new()
        {
            {
                typeof(BadHttpRequestException),
                new ProblemDetailsDefinition(400, "Invalid request", "request/invalid")
            },
            {
                typeof(UnauthorizedAccessFargoApplicationException),
                new ProblemDetailsDefinition(401, "Unauthorized", "auth/unauthorized")
            },
            {
                typeof(InvalidCredentialsFargoApplicationException),
                new ProblemDetailsDefinition(401, "Invalid credentials", "auth/invalid-credentials")
            },
            {
                typeof(InvalidPasswordFargoApplicationException),
                new ProblemDetailsDefinition(400, "Invalid password", "auth/invalid-password")
            },
            {
                typeof(WeakPasswordFargoApplicationException),
                new ProblemDetailsDefinition(400, "Weak password", "auth/weak-password")
            },
            {
                typeof(PasswordChangeRequiredFargoApplicationException),
                new ProblemDetailsDefinition(403, "Password change required", "auth/password-change-required")
            },
            {
                typeof(ArticleNotFoundFargoApplicationException),
                new ProblemDetailsDefinition(404, "Article not found", "article/not-found")
            },
            {
                typeof(UserNotFoundFargoApplicationException),
                new ProblemDetailsDefinition(404, "User not found", "user/not-found")
            },
            {
                typeof(ItemNotFoundFargoApplicationException),
                new ProblemDetailsDefinition(404, "Item not found", "item/not-found")
            },
            {
                typeof(UserGroupNotFoundFargoApplicationException),
                new ProblemDetailsDefinition(404, "User group not found", "user-group/not-found")
            },
            {
                typeof(UserNotAuthorizedFargoDomainException),
                new ProblemDetailsDefinition(403, "Forbidden", "user/forbidden")
            },
            {
                typeof(ArticleDeleteWithItemsAssociatedFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "article/delete-with-items")
            },
            {
                typeof(UserNameidAlreadyExistsDomainException),
                new ProblemDetailsDefinition(409, "Conflict", "user/nameid-already-exists")
            },
            {
                typeof(UserGroupNameidAlreadyExistsDomainException),
                new ProblemDetailsDefinition(409, "Conflict", "user-group/nameid-already-exists")
            },
            {
                typeof(UserCannotDeleteSelfFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-delete-self")
            },
            {
                typeof(UserCannotChangeOwnPermissionsFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-change-own-permissions")
            },
            {
                typeof(UserInactiveFargoDomainException),
                new ProblemDetailsDefinition(403, "User inactive", "user/inactive")
            },
            {
                typeof(PartitionNotFoundFargoApplicationException),
                new ProblemDetailsDefinition(404, "Partition not found", "partition/not-found")
            },
            {
                typeof(UserNotAuthorizedFargoApplicationException),
                new ProblemDetailsDefinition(403, "Forbidden", "user/forbidden")
            },
            {
                typeof(PartitionAccessDeniedFargoApplicationException),
                new ProblemDetailsDefinition(403, "Access denied", "partition/access-denied")
            },
            {
                typeof(PartitionedEntityAccessDeniedFargoApplicationException),
                new ProblemDetailsDefinition(403, "Access denied", "entity/access-denied")
            },
            {
                typeof(UserGroupInactiveFargoDomainException),
                new ProblemDetailsDefinition(403, "User group inactive", "user-group/inactive")
            },
            {
                typeof(PartitionCircularHierarchyFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "partition/circular-hierarchy")
            },
            {
                typeof(PartitionCannotBeOwnParentFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "partition/cannot-be-own-parent")
            },
            {
                typeof(DeleteMainAdminUserFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-delete-main-admin")
            },
            {
                typeof(DeleteDefaultAdministratorsUserGroupFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user-group/cannot-delete-default-admins")
            },
            {
                typeof(ChangeMainAdminUserPermissionsFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-change-main-admin-permissions")
            },
            {
                typeof(PartitionGlobalDeleteFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "partition/cannot-delete-global")
            },
            {
                typeof(UserCannotDeleteParentUserGroupFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user-group/cannot-delete-parent-group")
            }
        };

    /// <summary>
    /// Attempts to retrieve a <see cref="ProblemDetailsDefinition"/>
    /// associated with the specified exception type.
    /// </summary>
    /// <param name="exceptionType">
    /// The exception type to search for in the registry.
    /// </param>
    /// <param name="definition">
    /// When this method returns, contains the associated
    /// <see cref="ProblemDetailsDefinition"/> if the exception type
    /// is registered; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a mapping exists for the specified
    /// exception type; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetDefinition(
            Type exceptionType,
            out ProblemDetailsDefinition definition)
    {
        return map.TryGetValue(exceptionType, out definition!);
    }

    /// <summary>
    /// Gets the complete exception-to-problem-details mapping.
    /// </summary>
    /// <remarks>
    /// This property exposes the internal registry as a read-only dictionary,
    /// allowing inspection of all registered mappings without permitting
    /// external modification.
    /// </remarks>
    public static IReadOnlyDictionary<Type, ProblemDetailsDefinition> Map
        => map;
}
