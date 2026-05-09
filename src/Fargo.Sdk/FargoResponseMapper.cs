using Fargo.Sdk.Contracts;
using Fargo.Sdk.Contracts.Errors;
using Fargo.Sdk.Http;
using System.Net;

namespace Fargo.Sdk;

internal static class FargoResponseMapper
{
    public static FargoResponse<TResult> Map<TResult>(
        FargoSdkHttpResponse<TResult> response,
        TResult? fallbackResult = default)
    {
        var statusCode = (int)response.StatusCode;

        if (!response.IsSuccess)
        {
            return new FargoResponse<TResult>(default, CreateProblem(response.Problem, response.StatusCode), statusCode);
        }

        return new FargoResponse<TResult>(response.Data ?? fallbackResult, null, statusCode);
    }

    public static FargoResponse Map(FargoSdkHttpResponse<EmptyResult> response)
    {
        var statusCode = (int)response.StatusCode;

        return response.IsSuccess
            ? new FargoResponse(null, statusCode)
            : new FargoResponse(CreateProblem(response.Problem, response.StatusCode), statusCode);
    }

    public static FargoProblemDetails CreateProblem(FargoProblemDetails? problem, HttpStatusCode statusCode)
    {
        var status = (int)statusCode;

        if (problem is not null)
        {
            return new FargoProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Detail = problem.Detail,
                Status = problem.Status ?? status,
                Instance = problem.Instance,
                TraceId = problem.TraceId
            };
        }

        return new FargoProblemDetails
        {
            Title = ReasonPhrases.GetTitle(statusCode),
            Detail = ReasonPhrases.GetFallbackDetail(statusCode),
            Status = status
        };
    }
}

internal static class ReasonPhrases
{
    public static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        _ => $"HTTP {(int)statusCode}"
    };

    public static string GetFallbackDetail(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.NotFound
            ? "The requested resource was not found."
            : "An unexpected error occurred.";
}
