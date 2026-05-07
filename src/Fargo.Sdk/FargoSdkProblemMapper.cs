using Fargo.Sdk.Http;
using System.Net;

namespace Fargo.Sdk;

internal static class FargoSdkProblemMapper
{
    public static FargoSdkError Map(FargoProblemDetails? problem, HttpStatusCode? statusCode = null)
    {
        var type = problem?.Type switch
        {
            "auth/unauthorized" => FargoSdkErrorType.UnauthorizedAccess,
            "auth/invalid-credentials" => FargoSdkErrorType.InvalidCredentials,
            "auth/invalid-password" => FargoSdkErrorType.InvalidCredentials,
            "auth/password-change-required" => FargoSdkErrorType.PasswordChangeRequired,

            "article/not-found"
                or "barcode/not-found"
                or "item/not-found"
                or "user/not-found"
                or "partition/not-found"
                or "user-group/not-found" => FargoSdkErrorType.NotFound,

            "user/nameid-already-exists"
                or "user-group/nameid-already-exists"
                or "barcode/already-exists" => FargoSdkErrorType.Conflict,

            "user/forbidden"
                or "user/inactive"
                or "user-group/inactive"
                or "partition/access-denied"
                or "entity/access-denied" => FargoSdkErrorType.Forbidden,

            "request/invalid"
                or "auth/weak-password"
                or "user/invalid-nameid"
                or "article/delete-with-items"
                or "barcode/invalid-value"
                or "item/cannot-be-own-container"
                or "item/parent-is-not-container"
                or "item/circular-container-hierarchy"
                or "partition/circular-hierarchy"
                or "partition/cannot-be-own-parent"
                or "partition/cannot-delete-global"
                or "user/cannot-delete-self"
                or "user/cannot-delete-main-admin"
                or "user/cannot-change-own-permissions"
                or "user/cannot-change-main-admin-permissions"
                or "user-group/cannot-delete-default-admins"
                or "user-group/cannot-delete-parent-group" => FargoSdkErrorType.InvalidInput,

            _ => FargoSdkErrorType.Undefined
        };

        if (type == FargoSdkErrorType.Undefined && (statusCode == HttpStatusCode.NotFound || problem?.Status == 404))
        {
            type = FargoSdkErrorType.NotFound;
        }

        return new FargoSdkError(
            type,
            problem?.Detail ?? (type == FargoSdkErrorType.NotFound ? "The requested resource was not found." : "An unexpected error occurred."),
            problem?.Title,
            problem?.Type,
            problem?.Status ?? (statusCode is null ? null : (int)statusCode.Value),
            problem?.Instance,
            problem?.TraceId);
    }
}
