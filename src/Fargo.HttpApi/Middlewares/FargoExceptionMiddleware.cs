using Fargo.Application.Exceptions;
using Fargo.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Middlewares
{
    public sealed class FargoExceptionMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BadHttpRequestException ex)
            {
                await WriteProblemDetailsAsync(context, 400, "Invalid request", ex.Message, "request/invalid");
            }
            catch (UnauthorizedAccessFargoApplicationException ex)
            {
                await WriteProblemDetailsAsync(context, 401, "Unauthorized", ex.Message, "auth/unauthorized");
            }
            catch (ArticleNotFoundFargoApplicationException ex)
            {
                await WriteProblemDetailsAsync(context, 404, "Article not found", ex.Message, "article/not-found");
            }
            catch (UserNotFoundFargoApplicationException ex)
            {
                await WriteProblemDetailsAsync(context, 404, "User not found", ex.Message, "user/not-found");
            }
            catch (ItemNotFoundFargoApplicationException ex)
            {
                await WriteProblemDetailsAsync(context, 404, "Item not found", ex.Message, "item/not-found");
            }
            catch (UserNotAuthorizedFargoDomainException ex)
            {
                await WriteProblemDetailsAsync(context, 403, "Forbidden", ex.Message, "user/forbidden");
            }
            catch (ArticleDeleteWithItemsAssociatedFargoDomainException ex)
            {
                await WriteProblemDetailsAsync(context, 400, "Invalid operation", ex.Message, "article/delete-with-items");
            }
            catch (UserNameidAlreadyExistsDomainException ex)
            {
                await WriteProblemDetailsAsync(context, 409, "Conflict", ex.Message, "user/nameid-already-exists");
            }
            catch (Exception)
            {
                await WriteProblemDetailsAsync(context, 500, "Internal server error", "An unexpected error occurred.", "server/internal-error");
            }
        }

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
    }
}