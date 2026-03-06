using Fargo.Application.Exceptions;
using Fargo.Domain.Exceptions;

namespace Fargo.HttpApi.Middlewares
{
    public class FargoExceptionMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BadHttpRequestException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            catch (UnauthorizedAccessFargoApplicationException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            catch (ArticleNotFoundFargoApplicationException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (UserNotFoundFargoApplicationException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (ItemNotFoundFargoApplicationException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (UserNotAuthorizedFargoDomainException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            catch (ArticleDeleteWithItemsAssociatedFargoDomainException)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}