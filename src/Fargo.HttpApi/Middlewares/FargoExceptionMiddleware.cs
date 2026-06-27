namespace Fargo.HttpApi.Middlewares;

public sealed class FargoExceptionMiddleware
{
    public FargoExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        this.next = next;
        this.environment = environment;
    }

    private readonly RequestDelegate next;
    private readonly IHostEnvironment environment;

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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles an exception and writes the corresponding <see cref="FargoProblemDetails"/> response.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="exception">The exception to handle.</param>
    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        if (FargoProblemDetailsRegistry.TryGetDefinition(exception.GetType(), out var definition))
        {
            await WriteProblemDetailsAsync(
                context,
                definition.StatusCode,
                definition.Title,
                exception.Message,
                definition.Type,
                context.TraceIdentifier);

            return;
        }

        await WriteProblemDetailsAsync(
            context,
            500,
            "Internal server error",
            environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
            "server/internal-error",
            context.TraceIdentifier);
    }

    /// <summary>
    /// Writes a <see cref="FargoProblemDetails"/> response to the HTTP response stream.
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
        string type,
        string traceId)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new FargoProblemDetailsDto
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
