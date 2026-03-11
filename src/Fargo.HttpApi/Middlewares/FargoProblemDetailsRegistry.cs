using Fargo.Application.Exceptions;
using Fargo.Domain.Exceptions;

namespace Fargo.HttpApi.Middlewares;

public static class FargoProblemDetailsRegistry
{
    private static readonly IReadOnlyDictionary<Type, ProblemDetailsDefinition> map =
        new Dictionary<Type, ProblemDetailsDefinition>
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
                typeof(InvalidPasswordFargoApplicationException),
                new ProblemDetailsDefinition(400, "Invalid password", "auth/invalid-password")
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
            }
        };

    public static bool TryGetDefinition(
        Type exceptionType,
        out ProblemDetailsDefinition definition)
    {
        return map.TryGetValue(exceptionType, out definition!);
    }

    public static IReadOnlyDictionary<Type, ProblemDetailsDefinition> Map
        => map;
}