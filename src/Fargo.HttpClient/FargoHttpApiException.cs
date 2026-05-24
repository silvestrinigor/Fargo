using Fargo.HttpContracts;
using System.Net;

namespace Fargo.HttpClient;

public sealed class FargoHttpApiException : Exception
{
    public FargoHttpApiException(
        HttpStatusCode statusCode,
        string? reasonPhrase,
        string rawBody,
        FargoProblemDetailsDto? problemDetails)
        : base(CreateMessage(statusCode, reasonPhrase, problemDetails))
    {
        StatusCode = statusCode;
        RawBody = rawBody;
        ProblemDetails = problemDetails;
    }

    public HttpStatusCode StatusCode { get; }

    public string RawBody { get; }

    public FargoProblemDetailsDto? ProblemDetails { get; }

    private static string CreateMessage(
        HttpStatusCode statusCode,
        string? reasonPhrase,
        FargoProblemDetailsDto? problemDetails)
    {
        if (problemDetails is not null)
        {
            return $"{(int)statusCode} {problemDetails.Title}: {problemDetails.Detail}";
        }

        return $"{(int)statusCode} {reasonPhrase}";
    }
}
