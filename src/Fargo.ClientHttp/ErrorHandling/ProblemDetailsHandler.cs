using System.Net;
using System.Text.Json;

namespace Fargo.ClientHttp.ErrorHandling;

/// <summary>
/// Converts non-success responses into typed <see cref="FargoApiException"/>s: 400, 401 and 403
/// have dedicated types and any other error status gets the generic one. The message is the
/// problem details' detail (prefixed by its title when present); without a detail, a default.
/// </summary>
public sealed class ProblemDetailsHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var status = (int)response.StatusCode;

        var problem = await TryReadProblemDetailsAsync(response, cancellationToken)
            .ConfigureAwait(false);

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestApiException(
                                problem,
                                BuildMessage(
                                    problem,
                                    defaultMessage:
                                    "Bad request (400): the server rejected the request. "
                                    + "Check the command arguments and try again.")),
            HttpStatusCode.Unauthorized => new UnauthorizedApiException(
                                problem,
                                BuildMessage(
                                    problem,
                                    defaultMessage:
                                    "Unauthorized (401): authentication is required or your "
                                    + "session has expired. Run 'fargo identity login' or "
                                    + "provide --username and --password.")),
            HttpStatusCode.Forbidden => new ForbiddenApiException(
                                problem,
                                BuildMessage(
                                    problem,
                                    defaultMessage:
                                    "Forbidden (403): you do not have permission to perform "
                                    + "this action.")),
            _ => new FargoApiException(
                                status,
                                problem,
                                BuildMessage(
                                    problem,
                                    defaultMessage:
                                    $"The server returned an error ({status} "
                                    + $"{response.ReasonPhrase}).")),
        };
    }

    private static async Task<FargoProblemDetails?> TryReadProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (mediaType is not ("application/problem+json" or "application/json"))
        {
            return null;
        }

        try
        {
            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            if (stream.CanSeek && stream.Length == 0)
            {
                return null;
            }

            return await JsonSerializer
                .DeserializeAsync<FargoProblemDetails>(
                    stream,
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildMessage(
        FargoProblemDetails? problem,
        string defaultMessage)
    {
        if (problem is null)
        {
            return defaultMessage;
        }

        var hasTitle = !string.IsNullOrWhiteSpace(problem.Title);
        var hasDetail = !string.IsNullOrWhiteSpace(problem.Detail);

        // The server's "detail" carries the specific, actionable explanation
        // (e.g. "The provided credentials are invalid."). A bare response from
        // the authentication/authorization middleware only has a generic status
        // title (e.g. "Unauthorized") with no detail, in which case the
        // actionable default guidance is more helpful.
        if (!hasDetail)
        {
            return defaultMessage;
        }

        return hasTitle
            ? $"{problem.Title} {problem.Detail}"
            : problem.Detail!;
    }
}
