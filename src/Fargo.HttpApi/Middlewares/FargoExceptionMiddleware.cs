using Fargo.Application.Exceptions;
using Fargo.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Middlewares
{
    /// <summary>
    /// Middleware responsible for handling exceptions thrown during HTTP request processing.
    /// </summary>
    /// <remarks>
    /// This middleware converts known application and domain exceptions into standardized
    /// <see cref="ProblemDetails"/> responses using the <c>application/problem+json</c> format.
    ///
    /// Unknown exceptions are mapped to a generic <c>500 Internal Server Error</c> response.
    /// </remarks>
    public sealed class FargoExceptionMiddleware(RequestDelegate next)
    {
        private static readonly Dictionary<Type, ProblemDetailsDefinition> exceptionMap = new()
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
                typeof(UserCannotDeleteSelfFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-delete-self")
            },
            {
                typeof(UserCannotChangeOwnPermissionsFargoDomainException),
                new ProblemDetailsDefinition(400, "Invalid operation", "user/cannot-change-own-permissions")
            }
        };

        /// <summary>
        /// Processes the current HTTP request and handles exceptions thrown by downstream middleware.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles an exception and writes the corresponding <see cref="ProblemDetails"/> response.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <param name="exception">The exception to handle.</param>
        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            if (exceptionMap.TryGetValue(exception.GetType(), out var definition))
            {
                await WriteProblemDetailsAsync(
                    context,
                    definition.StatusCode,
                    definition.Title,
                    exception.Message,
                    definition.Type);

                return;
            }

            await WriteProblemDetailsAsync(
                context,
                500,
                "Internal server error",
                "An unexpected error occurred.",
                "server/internal-error");
        }

        /// <summary>
        /// Writes a <see cref="ProblemDetails"/> response to the HTTP response stream.
        /// </summary>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <param name="statusCode">The HTTP status code returned to the client.</param>
        /// <param name="title">A short human-readable summary of the problem.</param>
        /// <param name="detail">A detailed explanation of the error.</param>
        /// <param name="type">A machine-readable identifier describing the error category.</param>
        private static async Task WriteProblemDetailsAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            string type)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = type,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        /// <summary>
        /// Represents the metadata used to build a <see cref="ProblemDetails"/> response.
        /// </summary>
        /// <param name="StatusCode">The HTTP status code.</param>
        /// <param name="Title">The problem title.</param>
        /// <param name="Type">The machine-readable problem type.</param>
        private sealed record ProblemDetailsDefinition(
            int StatusCode,
            string Title,
            string Type);
    }
}