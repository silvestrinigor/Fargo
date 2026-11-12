using Fargo.Application.Idempotency;

namespace Fargo.ServiceHttp.Endpoints;

/// <summary>
/// The HTTP side of idempotency keys: the request header, the replay marker on the response, and the
/// documented error responses.
/// </summary>
internal static class IdempotencyEndpointExtensions
{
    public const string KeyHeaderName = "Idempotency-Key";

    public const string ReplayedHeaderName = "Idempotency-Replayed";

    /// <summary>
    /// Marks a response that was answered from a stored result instead of running the command again.
    /// </summary>
    public static void MarkIfReplayed(this HttpResponse response, IdempotencyOutcome outcome)
    {
        if (outcome.Replayed)
        {
            response.Headers[ReplayedHeaderName] = "true";
        }
    }

    public static RouteHandlerBuilder ProducesIdempotencyErrors(this RouteHandlerBuilder builder)
        => builder
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity);
}