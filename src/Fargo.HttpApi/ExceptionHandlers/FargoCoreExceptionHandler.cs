using Fargo.Core;
using Fargo.Core.Users;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoCoreExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FargoCoreException coreException)
        {
            ProblemDetails problem;

            switch (coreException)
            {
                case UserNameidAlreadyExistsFargoCoreException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "User nameid is already in use.",
                    };

                    problem.Extensions.Add("nameId", ex.Nameid);

                    break;

                case DeleteMainAdminUserFargoCoreException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Cannot delete main admin user.",
                    };

                    break;

                default:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Core exception."
                    };

                    break;
            }

            problem.Detail = coreException.Message;

            problem.Instance = httpContext.Request.Path;

            problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

            problem.Extensions.TryAdd("coreErrorType", coreException.ErrorType);

            return true;
        }

        return false;
    }
}
